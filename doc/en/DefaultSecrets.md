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

The result is computed once and cached, so the startup log, the health check and the administration UI always
report the same state.

## Where findings are visible

* **Startup log** - one entry per finding.
* **Health check** - the `default-secrets` check, tagged `ready`, reports `Degraded` while any finding remains and
  lists the offending keys in its description and data. `/health` returns `200` for a degraded state and `503` only
  for an unhealthy one; `/liveness` is not affected, it only runs checks tagged `live`.
* **Administration UI** - a banner in the application layout, shown to administrators **in `Production` only**.
  It is backed by `GET /api/security/get-default-secrets-report`, which requires the `admin` role and lists every
  setting together with the environment variable that overrides it. The endpoint reports findings in every
  environment and raises `showBanner` only in `Production`, so a developer running the sample configuration is not
  trained to ignore a permanent warning. To see the banner locally, start the service with
  `ASPNETCORE_ENVIRONMENT=Production`.

## Where the secrets are declared

Secret bearing settings are declared in one place:

* properties of the `*Settings` classes marked with `[SecretSetting]`
  (`Oip.Base/Settings/Attributes/SecretSettingAttribute.cs`);
* configuration keys that are not part of the settings object graph, listed in
  `KnownDefaultSecrets.RawSecrets` (`Oip.Base/Security/DefaultSecrets/KnownDefaultSecrets.cs`).

The shipped values themselves are constants of `KnownDefaultSecrets` and double as the defaults of the matching
properties, so a sample value is written down exactly once.

## Settings to override

| Configuration key | Environment variable | Where the sample value lives |
| --- | --- | --- |
| `SecurityService:ClientSecret` | `SecurityService__ClientSecret` | `KnownDefaultSecrets` (code default) |
| `SecurityService:AdminPassword` | `SecurityService__AdminPassword` | not shipped, set it yourself |
| `SecurityService:AuthTicketStore:RedisConnectionString` | `SecurityService__AuthTicketStore__RedisConnectionString` | `KnownDefaultSecrets` (code default) |
| `UserPhotoStorage:SecretKey` | `UserPhotoStorage__SecretKey` | `KnownDefaultSecrets` (code default) |
| `DiscussionAttachmentStorage:SecretKey` | `DiscussionAttachmentStorage__SecretKey` | `KnownDefaultSecrets` (code default) |
| `KeycloakSync:SharedSecret` | `KeycloakSync__SharedSecret` | `.oip-devcontainer/.env.example` and `realm-export.json` |
| `SmtpSettings:SmtpPassword` | `SmtpSettings__SmtpPassword` | not shipped, set it yourself |
| `Kestrel:Endpoints:Https:Certificate:Password` | `Kestrel__Endpoints__Https__Certificate__Password` | `appsettings.Development.json` |

Use environment variables, `dotnet user-secrets` during development, or your own secret store. `__` is the
environment variable separator that ASP.NET Core maps to `:`.

The sample values are no longer copied into every service. `SecurityService:ClientSecret`, the Redis connection
string and the object storage secret keys come from the code defaults in `KnownDefaultSecrets`, and
`KeycloakSync:SharedSecret` is supplied by the development container, where it has to match `realm-export.json`
anyway. Rotating a sample value means editing one place instead of hunting through eight `appsettings.json` files.

Changing a secret is described separately, per secret, in [Secret Rotation](./SecretRotation.md): which environment
variable owns it, what has to change inside Keycloak or MinIO, and what has to be restarted.

## CI guard

`.github/workflows/pullrequest.yml` runs `gitleaks` over the checked-out tree on every pull request and fails when
a new hardcoded secret appears. The sample credentials above are allowlisted **by value** in `.gitleaks.toml`, so
a new secret is caught wherever it is added - including inside a file that already carries a sample one. Adding a
value to that allowlist is a deliberate decision: it must be a sample credential an operator is expected to
override, never a real one.

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
