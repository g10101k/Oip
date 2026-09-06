# Default Secrets

The repository ships working sample credentials so that the development container starts out of the box. Those
values are public — every clone of the repository has them. Any installation that is used for real data must
override all of them.

## Detection

Every service runs a startup validator (`DefaultSecretsStartupTask`) that inspects the effective configuration and
reports each secret setting that

* still holds the value shipped with the repository, or
* is required but empty.

Findings are logged as `Warning` outside `Production` and as `Error` in `Production`. Each message names the
configuration key and the environment variable that overrides it.

## Where the secrets are declared

Secret bearing settings are declared in one place:

* properties of the `*Settings` classes marked with `[SecretSetting]`
  (`Oip.Base/Settings/Attributes/SecretSettingAttribute.cs`);
* configuration keys that are not part of the settings object graph, listed in
  `KnownDefaultSecrets.RawSecrets` (`Oip.Base/Security/DefaultSecrets/KnownDefaultSecrets.cs`).

The shipped values themselves are constants of `KnownDefaultSecrets`, so rotating a sample value means editing a
single file plus the `appsettings.json` files that carry it.

## Settings to override

| Configuration key | Environment variable | Shipped default |
| --- | --- | --- |
| `SecurityService:ClientSecret` | `SecurityService__ClientSecret` | Keycloak client secret of the sample realm |
| `SecurityService:AdminPassword` | `SecurityService__AdminPassword` | `P@ssw0rd` |
| `UserPhotoStorage:SecretKey` | `UserPhotoStorage__SecretKey` | `P@ssw0rd` |
| `DiscussionAttachmentStorage:SecretKey` | `DiscussionAttachmentStorage__SecretKey` | `P@ssw0rd` |
| `KeycloakSync:SharedSecret` | `KeycloakSync__SharedSecret` | `change-me-keycloak-events` |
| `SmtpSettings:SmtpPassword` | `SmtpSettings__SmtpPassword` | sample data protection blob |
| `Kestrel:Endpoints:Https:Certificate:Password` | `Kestrel__Endpoints__Https__Certificate__Password` | `P@ssw0rd` |

Use environment variables, `dotnet user-secrets` during development, or your own secret store. `__` is the
environment variable separator that ASP.NET Core maps to `:`.

## Configuration

The validator is configured through the `Security` section:

```json
{
  "Security": {
    "ValidateDefaultSecrets": true,
    "FailOnDefaultSecrets": false,
    "IgnoredSecretKeys": []
  }
}
```

* `ValidateDefaultSecrets` — turns the check off entirely. Default `true`.
* `FailOnDefaultSecrets` — when `true` and the environment is `Production`, the service refuses to start while any
  finding remains. Default `false`.
* `IgnoredSecretKeys` — configuration keys excluded from the check, for a secret that is intentionally unused.
