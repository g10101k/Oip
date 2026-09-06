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

## Где объявлены секреты

Секретные настройки объявлены в одном месте:

* свойства классов `*Settings`, помеченные атрибутом `[SecretSetting]`
  (`Oip.Base/Settings/Attributes/SecretSettingAttribute.cs`);
* ключи конфигурации вне графа настроек — в списке `KnownDefaultSecrets.RawSecrets`
  (`Oip.Base/Security/DefaultSecrets/KnownDefaultSecrets.cs`).

Сами значения по умолчанию — константы `KnownDefaultSecrets`, поэтому ротация тестового значения затрагивает один
файл кода плюс те `appsettings.json`, где оно продублировано.

## Что нужно переопределить

| Ключ конфигурации | Переменная окружения | Значение по умолчанию |
| --- | --- | --- |
| `SecurityService:ClientSecret` | `SecurityService__ClientSecret` | client secret тестового realm Keycloak |
| `SecurityService:AdminPassword` | `SecurityService__AdminPassword` | `P@ssw0rd` |
| `UserPhotoStorage:SecretKey` | `UserPhotoStorage__SecretKey` | `P@ssw0rd` |
| `DiscussionAttachmentStorage:SecretKey` | `DiscussionAttachmentStorage__SecretKey` | `P@ssw0rd` |
| `KeycloakSync:SharedSecret` | `KeycloakSync__SharedSecret` | `change-me-keycloak-events` |
| `SmtpSettings:SmtpPassword` | `SmtpSettings__SmtpPassword` | тестовый data protection blob |
| `Kestrel:Endpoints:Https:Certificate:Password` | `Kestrel__Endpoints__Https__Certificate__Password` | `P@ssw0rd` |

Используйте переменные окружения, `dotnet user-secrets` при разработке или собственное хранилище секретов. `__` —
разделитель в переменных окружения, который ASP.NET Core отображает в `:`.

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
