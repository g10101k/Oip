# Секреты по умолчанию

В репозитории лежат рабочие тестовые учётные данные, чтобы dev-контейнер запускался без настройки. Эти значения
публичны — они есть в каждом клоне репозитория. Любая установка, работающая с реальными данными, обязана
переопределить их все.

## Обнаружение

Каждый сервис запускает стартовый валидатор (`DefaultSecretsStartupTask`), который проверяет фактическую
конфигурацию и сообщает о каждой секретной настройке, которая

* всё ещё содержит значение из репозитория, либо
* обязательна, но пуста.

Находки логируются как `Warning` вне `Production` и как `Error` в `Production`. В сообщении указывается ключ
конфигурации и переменная окружения, которой его можно переопределить.

Результат вычисляется один раз и кэшируется, поэтому стартовый лог, health check и админский интерфейс всегда
показывают одно и то же состояние.

## Где видны находки

* **Стартовый лог** - по одной записи на каждую находку.
* **Health check** - проверка `default-secrets` с тегом `ready` возвращает `Degraded`, пока остаётся хотя бы одна
  находка, и перечисляет проблемные ключи в описании и данных. `/health` при `Degraded` отдаёт `200` и `503` только
  при `Unhealthy`; `/liveness` не затрагивается - он выполняет только проверки с тегом `live`.
* **Админский интерфейс** - баннер в layout приложения, показывается администраторам **только в `Production`**.
  Данные берутся из `GET /api/security/get-default-secrets-report`, эндпоинт требует роль `admin` и перечисляет
  каждую настройку вместе с переменной окружения для переопределения. Находки эндпоинт отдаёт в любом окружении,
  но флаг `showBanner` поднимает только в `Production` - чтобы разработчик на тестовой конфигурации не привыкал
  игнорировать постоянное предупреждение. Чтобы посмотреть баннер локально, запустите сервис с
  `ASPNETCORE_ENVIRONMENT=Production`.

## Где объявлены секреты

Секретные настройки объявлены в одном месте:

* свойства классов `*Settings`, помеченные атрибутом `[SecretSetting]`
  (`Oip.Base/Settings/Attributes/SecretSettingAttribute.cs`);
* ключи конфигурации вне графа настроек — в списке `KnownDefaultSecrets.RawSecrets`
  (`Oip.Base/Security/DefaultSecrets/KnownDefaultSecrets.cs`).

Сами значения по умолчанию — константы `KnownDefaultSecrets`, и они же служат дефолтами соответствующих свойств,
поэтому тестовое значение задано ровно один раз.

## Что нужно переопределить

| Ключ конфигурации | Переменная окружения | Где лежит тестовое значение |
| --- | --- | --- |
| `SecurityService:ClientSecret` | `SecurityService__ClientSecret` | `KnownDefaultSecrets` (дефолт в коде) |
| `SecurityService:AdminPassword` | `SecurityService__AdminPassword` | не поставляется, задайте сами |
| `SecurityService:AuthTicketStore:RedisConnectionString` | `SecurityService__AuthTicketStore__RedisConnectionString` | `appsettings.json` каждого сервиса |
| `UserPhotoStorage:SecretKey` | `UserPhotoStorage__SecretKey` | `KnownDefaultSecrets` (дефолт в коде) |
| `DiscussionAttachmentStorage:SecretKey` | `DiscussionAttachmentStorage__SecretKey` | `KnownDefaultSecrets` (дефолт в коде) |
| `KeycloakSync:SharedSecret` | `KeycloakSync__SharedSecret` | `.oip-devcontainer/dev.yml` и `realm-export.json` |
| `SmtpSettings:SmtpPassword` | `SmtpSettings__SmtpPassword` | не поставляется, задайте сами |
| `Kestrel:Endpoints:Https:Certificate:Password` | `Kestrel__Endpoints__Https__Certificate__Password` | `appsettings.Development.json` |

Используйте переменные окружения, `dotnet user-secrets` при разработке или собственное хранилище секретов. `__` —
разделитель в переменных окружения, который ASP.NET Core отображает в `:`.

Тестовые значения больше не копируются в каждый сервис. `SecurityService:ClientSecret` и секретные ключи
объектного хранилища берутся из дефолтов в `KnownDefaultSecrets`, а `KeycloakSync:SharedSecret` задаёт
dev-контейнер, где он всё равно обязан совпадать с `realm-export.json`. Ротация тестового значения теперь
затрагивает одно место вместо восьми файлов `appsettings.json`.

## Гард в CI

`.github/workflows/pullrequest.yml` на каждый pull request прогоняет `gitleaks` по рабочему дереву и падает, если
появился новый захардкоженный секрет. Перечисленные выше тестовые значения занесены в allowlist **по значению** в
`.gitleaks.toml`, поэтому новый секрет будет пойман где угодно — в том числе в файле, где уже лежит тестовый.
Добавление значения в этот allowlist — осознанное решение: там место только тестовым учётным данным, которые
оператор обязан переопределить, но не настоящим.


## Настройка

Валидатор настраивается секцией `Security`:

```json
{
  "Security": {
    "ValidateDefaultSecrets": true,
    "FailOnDefaultSecrets": false,
    "IgnoredSecretKeys": []
  }
}
```

* `ValidateDefaultSecrets` — полностью отключает проверку. По умолчанию `true`.
* `FailOnDefaultSecrets` — при `true` и окружении `Production` сервис отказывается стартовать, пока остаётся хотя
  бы одна находка. По умолчанию `false`.
* `IgnoredSecretKeys` — ключи, исключённые из проверки, для намеренно неиспользуемого секрета.
