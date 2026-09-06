# Ротация секретов

[Секреты по умолчанию](./DefaultSecrets.md) описывают, как **обнаруживается** неизменённое тестовое значение. Этот
документ описывает, как оно **меняется**.

Почти каждый секрет существует в двух местах: во внешнем сервисе, которому он принадлежит, и в конфигурации
приложения, которое с ним аутентифицируется. Изменение одной половины ломает установку, поэтому каждый секрет
задаётся ровно одной переменной окружения, а здесь описано, что ещё должно произойти при её изменении.

## Единый источник истины

`.oip-devcontainer/.env.example` перечисляет все переменные. Скопируйте файл один раз и правьте копию:

````shell
cd .oip-devcontainer
cp .env.example .env
````

Docker Compose читает `.env` из каталога compose-файла автоматически, подставляет значения в `dev.yml`, а `dev.yml`
передаёт их приложениям как переопределения конфигурации ASP.NET Core. `.env` добавлен в `.gitignore`.

| Переменная | Кому принадлежит | Кто использует |
| --- | --- | --- |
| `POSTGRES_USER`, `POSTGRES_PASSWORD` | контейнер `postgres` | Keycloak (`KC_DB_*`) |
| `KEYCLOAK_ADMIN_USERNAME`, `KEYCLOAK_ADMIN_PASSWORD` | контейнер `keycloak` | операторы, `apply-secrets.sh` |
| `KEYCLOAK_CLIENT_SECRET` | база данных Keycloak | `SecurityService:ClientSecret` |
| `KEYCLOAK_EVENTS_SHARED_SECRET` | атрибут realm в Keycloak | `KeycloakSync:SharedSecret` |
| `REDIS_PASSWORD` | контейнер `redis` | `SecurityService:AuthTicketStore:RedisConnectionString` |
| `MINIO_ROOT_USER`, `MINIO_ROOT_PASSWORD` | контейнер `minio` | `apply-secrets.sh` |
| `MINIO_ACCESS_KEY`, `MINIO_SECRET_KEY` | база учётных записей MinIO | `UserPhotoStorage:*`, `DiscussionAttachmentStorage:*` |
| `CLICKHOUSE_USER`, `CLICKHOUSE_PASSWORD` | контейнер `clickhouse` | `RtsConnectionString` в `Oip.Rtds` |
| `GRAFANA_ADMIN_PASSWORD` | контейнер `grafana` | операторы |
| `DEV_CERT_PASSWORD` | файл `https/oip.pfx` | `Kestrel__Certificates__Default__Password` |

Ни одно из этих значений больше не записано в `appsettings.json` сервисов. Тестовые значения живут в
`KnownDefaultSecrets` (`Oip.Base/Security/DefaultSecrets/KnownDefaultSecrets.cs`) как дефолты в коде и в
`.env.example`, то есть по одному разу на каждую сторону.

## `apply-secrets.sh`

Три секрета сервисы хранят у себя, а не берут из окружения контейнера, поэтому пересоздание контейнера их не
меняет:

* client secret клиента `oip-backend` — в БД Keycloak (`KC_DB: postgres`, база `keycloak`);
* shared secret listener’а событий, атрибут realm `_providerConfig.ext-event-http.0` — там же;
* учётная запись MinIO, под которой работают приложения — во внутреннем хранилище MinIO на томе `minio_data`.

Этим они отличаются от, например, `REDIS_PASSWORD`, который передаётся в команду запуска Redis при каждом старте.
`.oip-devcontainer/apply-secrets.sh` записывает все три в запущенный стек, беря значения из `.env`. Скрипт
идемпотентен.

````shell
cd .oip-devcontainer
./apply-secrets.sh
docker compose -f dev.yml up -d --force-recreate oip
````

Ниже описано, что именно делает скрипт и как выполнить то же самое вручную в установке, отличной от dev-контейнера.

## Client secret Keycloak

`SecurityService:ClientSecret` — секрет конфиденциального клиента `oip-backend`. Приложения и Keycloak должны
использовать одно и то же значение.

`.oip-devcontainer/keycloak/realm-export.json` импортируется **только в пустую базу данных Keycloak**, при первом
старте. Дальше секрет живёт в базе Keycloak, и правка файла экспорта ничего не меняет. Подстановку переменных
окружения в realm-файлы Keycloak тоже не делает: плейсхолдер `${env.VAR:default}` в realm-файле раскрывается в
значение по умолчанию — проверено на Keycloak 26.6.3. Поэтому в экспорте остаётся тестовый литерал, а ротация
выполняется через Admin API.

Смена значения через `kcadm.sh` внутри контейнера Keycloak:

````shell
docker compose -f dev.yml exec keycloak /opt/keycloak/bin/kcadm.sh config credentials \
  --server http://localhost:8080 --realm master --user admin --password "$KEYCLOAK_ADMIN_PASSWORD"

CLIENT_UUID=$(docker compose -f dev.yml exec -T keycloak /opt/keycloak/bin/kcadm.sh get clients \
  -r oip -q clientId=oip-backend --fields id --format csv --noquotes | tr -d '\r\n')

docker compose -f dev.yml exec -T keycloak /opt/keycloak/bin/kcadm.sh update "clients/$CLIENT_UUID" \
  -r oip -s "secret=$KEYCLOAK_CLIENT_SECRET"
````

Чтобы секрет сгенерировал сам Keycloak, вызовите `POST clients/$CLIENT_UUID/client-secret` и прочитайте новое
значение:

````shell
docker compose -f dev.yml exec -T keycloak /opt/keycloak/bin/kcadm.sh create \
  "clients/$CLIENT_UUID/client-secret" -r oip
docker compose -f dev.yml exec -T keycloak /opt/keycloak/bin/kcadm.sh get \
  "clients/$CLIENT_UUID/client-secret" -r oip
````

Порядок действий:

1. Задать новое значение в Keycloak.
2. Записать то же значение в `KEYCLOAK_CLIENT_SECRET` в `.env`.
3. Пересоздать контейнеры приложений: `docker compose -f dev.yml up -d --force-recreate oip`.

Между шагами 1 и 3 вход не работает — Keycloak отклоняет старый секрет на token endpoint. Существующие сессии
продолжают работать, их тикеты уже сохранены.

## Shared secret событий Keycloak

`KeycloakSync:SharedSecret` подписывает заголовок `X-Keycloak-Signature` в webhook событий. Его пара — поле
`sharedSecret` в атрибуте realm `_providerConfig.ext-event-http.0`, который настраивается по документу
[Синхронизация пользователей Keycloak](./KeycloakUserSync.md).

Значение атрибута — JSON-документ, а ключ атрибута содержит точки, поэтому `kcadm.sh -s ключ=значение` его не
адресует. Атрибут переписывается целиком: получите realm, замените `sharedSecret` и отправьте обратно — именно так
делает `apply-secrets.sh`.

````shell
docker compose -f dev.yml exec -T keycloak /opt/keycloak/bin/kcadm.sh get realms/oip > realm.json
# заменить sharedSecret в realm.json
docker compose -f dev.yml exec -T keycloak /opt/keycloak/bin/kcadm.sh update realms/oip -f - < realm.json
````

Keycloak применяет конфигурацию listener сразу, поэтому меняйте атрибут realm и конфигурацию приложения вместе и
пересоздавайте контейнеры приложений. События, отправленные в промежутке, принимающий endpoint отклонит с `401`;
listener phasetwo повторит их.

## Пароль Redis

`REDIS_PASSWORD` передаётся в `redis-server --requirepass` и подставляется в строку подключения, которой пользуются
приложения, — одна правка меняет обе стороны. Внутри Redis пароль не хранится, отдельного шага ротации в сервисе
нет.

````shell
cd .oip-devcontainer
# изменить REDIS_PASSWORD в .env
docker compose -f dev.yml up -d --force-recreate redis oip
````

Пересоздавайте Redis и приложения вместе. Тикеты аутентификации — это записи кэша: перезапуск Redis разлогинивает
всех, но ничего не портит.

Если изменить только одну сторону, приложения продолжат обслуживать запросы —
`DistributedAuthenticationTicketStore` переключится на in-memory хранилище, — а в стартовом логе будет причина:

````text
Redis rejected the configured credentials. Check the password in
'SecurityService:AuthTicketStore:RedisConnectionString' (REDIS_PASSWORD) against the credentials Redis was started
with.
````

Сообщение выдаёт `RedisSecretConnectivityProbe` (`Oip.Base/Security/Connectivity/`) — стартовая проверка, которая
открывает одно аутентифицированное подключение. Недоступность сервиса логируется как предупреждение; ошибкой
считается только отклонение учётных данных.

## Учётные данные MinIO

Контейнер `minio` запускается с **root-аккаунтом** (`MINIO_ROOT_USER` / `MINIO_ROOT_PASSWORD`). Этот аккаунт владеет
инстансом: он может удалить любой bucket и создавать пользователей. Приложения не должны работать под ним.

Заведите приложениям отдельную учётную запись. Задайте `MINIO_ACCESS_KEY` и `MINIO_SECRET_KEY`, отличные от
root-значений, и выполните `./apply-secrets.sh` — или сделайте это вручную:

````shell
docker compose -f dev.yml exec -T minio mc alias set local http://localhost:9000 \
  "$MINIO_ROOT_USER" "$MINIO_ROOT_PASSWORD"

docker compose -f dev.yml exec -T minio mc admin user add local "$MINIO_ACCESS_KEY" "$MINIO_SECRET_KEY"
docker compose -f dev.yml exec -T minio mc admin policy attach local readwrite --user "$MINIO_ACCESS_KEY"
````

`readwrite` — встроенная политика, она распространяется на все bucket. Чтобы ограничить учётную запись двумя
bucket, которыми реально пользуются приложения, создайте свою политику и назначьте её:

````shell
cat > /tmp/oip-storage.json <<'JSON'
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": ["s3:*"],
      "Resource": [
        "arn:aws:s3:::oip-user-photos", "arn:aws:s3:::oip-user-photos/*",
        "arn:aws:s3:::oip-discussion-attachments", "arn:aws:s3:::oip-discussion-attachments/*"
      ]
    }
  ]
}
JSON
docker compose -f dev.yml cp /tmp/oip-storage.json minio:/tmp/oip-storage.json
docker compose -f dev.yml exec -T minio mc admin policy create local oip-storage /tmp/oip-storage.json
docker compose -f dev.yml exec -T minio mc admin policy attach local oip-storage --user "$MINIO_ACCESS_KEY"
````

`mc admin user add` для существующей учётной записи заменяет её secret key — так выполняется последующая ротация.
После этого пересоздайте контейнеры приложений; MinIO подписывает каждый запрос отдельно, никакие сессии смену не
переживают.

Смена root-пароля — отдельный шаг: измените `MINIO_ROOT_PASSWORD` и пересоздайте контейнер `minio`. Учётные записи,
созданные под старым root-аккаунтом, продолжат работать — они хранятся в собственной базе MinIO.

Отклонённую пару ключей на старте сообщают пробы объектного хранилища, зарегистрированные в `Oip.Users.Base` и
`Oip.Discussions.Base`:

````text
MinIO (UserPhotoStorage) rejected the configured credentials. Check 'UserPhotoStorage:AccessKey' and
'UserPhotoStorage:SecretKey' (MINIO_ACCESS_KEY, MINIO_SECRET_KEY) against the credentials MinIO was started with.
````

## Сертификат разработки

`DEV_CERT_PASSWORD` — пароль файла `https/oip.pfx`. Он задаётся при генерации файла командой
`openssl pkcs12 -export -passout pass:...` из `.oip-devcontainer/README.md`. Перегенерируйте файл с новым паролем и
пересоздайте контейнеры приложений.

## Вне dev-контейнера

Compose-файл и `.env` — механизм dev-контейнера. В любой другой установке те же переменные задаёт развёртывание:
секреты Kubernetes, строки `Environment=` в unit-файле systemd или хранилище секретов платформы. Со стороны
приложения каждый секрет — это ключ конфигурации ASP.NET Core, поэтому имя переменной окружения получается заменой
`:` на `__`:

| Ключ конфигурации | Переменная окружения |
| --- | --- |
| `SecurityService:ClientSecret` | `SecurityService__ClientSecret` |
| `SecurityService:AuthTicketStore:RedisConnectionString` | `SecurityService__AuthTicketStore__RedisConnectionString` |
| `KeycloakSync:SharedSecret` | `KeycloakSync__SharedSecret` |
| `UserPhotoStorage:AccessKey`, `UserPhotoStorage:SecretKey` | `UserPhotoStorage__AccessKey`, `UserPhotoStorage__SecretKey` |
| `DiscussionAttachmentStorage:AccessKey`, `DiscussionAttachmentStorage:SecretKey` | `DiscussionAttachmentStorage__AccessKey`, `DiscussionAttachmentStorage__SecretKey` |

При разработке альтернатива — `dotnet user-secrets`: значения вообще не попадают в рабочее дерево.

## Проверка после ротации

Убедитесь, что:

* стартовый лог каждого сервиса содержит `Default secret validation passed` — см.
  [Секреты по умолчанию](./DefaultSecrets.md);
* в стартовом логе нет строк `rejected the configured credentials`;
* `/health` показывает проверку `default-secrets` как healthy, а не degraded;
* новый вход выполняется успешно — это проверяет client secret Keycloak;
* загрузка фотографии пользователя проходит — это проверяет учётные данные MinIO.
