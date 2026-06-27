# Синхронизация пользователей Keycloak

В OIP Keycloak является источником идентификации пользователей: он отвечает за аутентификацию, учетные записи, email,
имя, фамилию, статус пользователя и роли. Внутренние приложения могут хранить расширенный профиль пользователя:
настройки, фото, локальные связи с бизнес-объектами и другую прикладную информацию.

## Общая схема

Синхронизация устроена как pull-after-event:

````text
Keycloak admin event -> webhook в OIP -> GET пользователя из Keycloak Admin API -> обновление локальной БД
````

Событие Keycloak используется как сигнал, что пользователя нужно перечитать. Сам payload события не считается надежным
источником данных.

## Компоненты

- `p2-inc/keycloak-events` - Keycloak Event Listener SPI, который отправляет события во внешний HTTP endpoint.
- `ext-event-http` - provider id HTTP-отправителя из расширения.
- `KeycloakEventsController` - внутренний webhook endpoint OIP.
- `UserSyncService` - сервис синхронизации пользователя из Keycloak в локальную БД.
- `KeycloakEvents:SharedSecret` - общий секрет для HMAC-подписи webhook-запроса.

Webhook endpoint:

````text
POST /api/keycloak-events/receive-keycloak-event
````

В dev standalone режиме endpoint обычно находится на хостовой
машине: https://host.docker.internal:5002/api/keycloak-events/receive-keycloak-event

Если используется отдельный сервис `Oip.Users`, target URI должен указывать на порт
`5005`: https://host.docker.internal:5005/api/keycloak-events/receive-keycloak-event

## Установка расширения Keycloak

В dev окружении Keycloak собирается из `.oip-devcontainer/keycloak/Dockerfile`. Dockerfile добавляет jar расширения:
`io.phasetwo.keycloak:keycloak-events:0.61`

После изменения Dockerfile или версии расширения нужно пересобрать Keycloak:

````shell
docker compose -f .oip-devcontainer/dev.yml build keycloak
docker compose -f .oip-devcontainer/dev.yml up -d --force-recreate keycloak
````

## Настройка listener в realm

Для работы HTTP provider нужно:

- включить listener `ext-event-http`;
- включить admin events;
- задать атрибут `_providerConfig.ext-event-http.0`;
- использовать тот же `sharedSecret`, что и в настройках приложения.

Войти в `kcadm.sh`:

````shell
docker compose -f .oip-devcontainer/dev.yml exec keycloak /opt/keycloak/bin/kcadm.sh config credentials \
  --server https://localhost:8443 \
  --realm master \
  --user admin \
  --password 'P@ssw0rd'
````

Включить listener и admin events:

````shell
docker compose -f .oip-devcontainer/dev.yml exec keycloak /opt/keycloak/bin/kcadm.sh update realms/oip \
  -s 'eventsEnabled=false' \
  -s 'adminEventsEnabled=true' \
  -s 'adminEventsDetailsEnabled=true' \
  -s 'eventsListeners=["ext-event-http","jboss-logging"]'
````

Задать HTTP provider config для приложения на хосте:

````shell
docker compose -f .oip-devcontainer/dev.yml exec keycloak /opt/keycloak/bin/kcadm.sh update realms/oip \
  -s 'attributes._providerConfig.ext-event-http.0={"targetUri":"https://host.docker.internal:5002/api/keycloak-events/receive-keycloak-event","sharedSecret":"change-me-keycloak-events","retry":true}'
````

Параметр `retry=true` включает повторную отправку на стороне `ext-event-http`, если HTTP-доставка события завершилась
ошибкой. Это retry provider-а Keycloak, а не очередь OIP и не бизнес-повтор синхронизации.

Проверить текущую конфигурацию realm:

````shell
docker compose -f .oip-devcontainer/dev.yml exec keycloak /opt/keycloak/bin/kcadm.sh get realms/oip
````

Если realm уже существует в БД Keycloak, изменения в `.oip-devcontainer/keycloak/realm-export.json` не применяются
автоматически. Нужно обновить realm через Admin UI, через `kcadm.sh`, либо пересоздать volume БД Keycloak.

Я рекомендую удалить БД и создать новую пустую.

## Настройки приложения

В `appsettings.json` должны быть включены Keycloak events:

````json
{
  "KeycloakEvents": {
    "Enabled": true,
    "SharedSecret": "change-me-keycloak-events"
  }
}
````

`SharedSecret` должен совпадать со значением `sharedSecret` в `_providerConfig.ext-event-http.0`.

## Диагностика

Если webhook не вызывается:

- проверить, что Keycloak image пересобран и jar расширения находится в `/opt/keycloak/providers`;
- проверить, что в realm есть listener `ext-event-http`;
- проверить `_providerConfig.ext-event-http.0`;
- проверить, что target URI доступен из контейнера Keycloak;
- проверить HMAC secret.

Если запросы приходят, но слишком часто:

- убедиться, что `eventsEnabled=false`, если нужны только admin changes;
- проверить, что webhook игнорирует события не по `users/{id}`;
- смотреть в логах `Ignored unrelated Keycloak event` и `Ignored unchanged Keycloak user`;
- убедиться, что локальный обработчик `UserUpdated` не пишет обратно в Keycloak без сравнения изменений.

Если есть TLS ошибка:

- проверить issuer и SAN сертификата backend;
- убедиться, что backend использует `.oip-devcontainer/https/oip.pfx`;
- перезапустить Keycloak после изменения CA;
- проверить `KC_TRUSTSTORE_PATHS` в `.oip-devcontainer/dev.yml`.
