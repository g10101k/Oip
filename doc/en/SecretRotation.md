# Secret Rotation

[Default Secrets](./DefaultSecrets.md) explains how an unchanged sample credential is **detected**. This document
explains how one is **changed**.

Almost every secret here exists in two places: inside the external service that owns it, and inside the
application configuration that authenticates with it. Changing one half breaks the installation, so each secret is
defined by exactly one environment variable, and this document says what else has to happen when that variable
changes.

## The single source of truth

`.oip-devcontainer/.env.example` lists every variable. Copy it once and edit the copy:

````shell
cd .oip-devcontainer
cp .env.example .env
````

Docker Compose reads `.env` from the directory of the compose file automatically, substitutes the values into
`dev.yml`, and `dev.yml` passes them on to the applications as ASP.NET Core configuration overrides. `.env` is
git-ignored.

| Variable | Owned by | Consumed by |
| --- | --- | --- |
| `POSTGRES_USER`, `POSTGRES_PASSWORD` | `postgres` container | Keycloak (`KC_DB_*`) |
| `KEYCLOAK_ADMIN_USERNAME`, `KEYCLOAK_ADMIN_PASSWORD` | `keycloak` container | operators, `apply-secrets.sh` |
| `KEYCLOAK_CLIENT_SECRET` | Keycloak database | `SecurityService:ClientSecret` |
| `KEYCLOAK_EVENTS_SHARED_SECRET` | Keycloak realm attribute | `KeycloakSync:SharedSecret` |
| `REDIS_PASSWORD` | `redis` container | `SecurityService:AuthTicketStore:RedisConnectionString` |
| `MINIO_ROOT_USER`, `MINIO_ROOT_PASSWORD` | `minio` container | `apply-secrets.sh` |
| `MINIO_ACCESS_KEY`, `MINIO_SECRET_KEY` | MinIO account database | `UserPhotoStorage:*`, `DiscussionAttachmentStorage:*` |
| `CLICKHOUSE_USER`, `CLICKHOUSE_PASSWORD` | `clickhouse` container | `Oip.Rtds` `RtsConnectionString` |
| `GRAFANA_ADMIN_PASSWORD` | `grafana` container | operators |
| `DEV_CERT_PASSWORD` | `https/oip.pfx` | `Kestrel__Certificates__Default__Password` |

None of these values is written into a service's `appsettings.json` any more. The sample values live in
`KnownDefaultSecrets` (`Oip.Base/Security/DefaultSecrets/KnownDefaultSecrets.cs`) as code defaults and in
`.env.example`, so a sample value is written down once per side.

## `apply-secrets.sh`

Three secrets are kept by the services themselves rather than read from a container's environment, so recreating
the container does not move them:

* the secret of the `oip-backend` client - in the Keycloak database (`KC_DB: postgres`, database `keycloak`);
* the event listener shared secret, the realm attribute `_providerConfig.ext-event-http.0` - in the same database;
* the MinIO account the applications authenticate with - in MinIO's own store on the `minio_data` volume.

That sets them apart from, say, `REDIS_PASSWORD`, which is passed to Redis's start command on every start.
`.oip-devcontainer/apply-secrets.sh` writes all three into the running stack from `.env`. It is idempotent.

````shell
cd .oip-devcontainer
./apply-secrets.sh
docker compose -f dev.yml up -d --force-recreate oip
````

The sections below describe what the script does, and how to do it by hand against an installation that is not the
development container.

## Keycloak client secret

`SecurityService:ClientSecret` is the secret of the `oip-backend` confidential client. Both the applications and
Keycloak have to agree on it.

`.oip-devcontainer/keycloak/realm-export.json` is imported **only into an empty Keycloak database**, on the first
start. Afterwards the secret lives in the Keycloak database, and editing the export file changes nothing. Keycloak
does not substitute environment variables into realm files either - a `${env.VAR:default}` placeholder in a realm
file resolves to its default, verified against Keycloak 26.6.3 - so the export keeps the sample literal and the
rotation happens through the Admin API.

Rotate it with `kcadm.sh` inside the Keycloak container:

````shell
docker compose -f dev.yml exec keycloak /opt/keycloak/bin/kcadm.sh config credentials \
  --server http://localhost:8080 --realm master --user admin --password "$KEYCLOAK_ADMIN_PASSWORD"

CLIENT_UUID=$(docker compose -f dev.yml exec -T keycloak /opt/keycloak/bin/kcadm.sh get clients \
  -r oip -q clientId=oip-backend --fields id --format csv --noquotes | tr -d '\r\n')

docker compose -f dev.yml exec -T keycloak /opt/keycloak/bin/kcadm.sh update "clients/$CLIENT_UUID" \
  -r oip -s "secret=$KEYCLOAK_CLIENT_SECRET"
````

To let Keycloak generate the secret instead of choosing one, `POST clients/$CLIENT_UUID/client-secret` and read the
new value back:

````shell
docker compose -f dev.yml exec -T keycloak /opt/keycloak/bin/kcadm.sh create \
  "clients/$CLIENT_UUID/client-secret" -r oip
docker compose -f dev.yml exec -T keycloak /opt/keycloak/bin/kcadm.sh get \
  "clients/$CLIENT_UUID/client-secret" -r oip
````

Order:

1. Set the new value in Keycloak.
2. Put the same value into `KEYCLOAK_CLIENT_SECRET` in `.env`.
3. Recreate the application containers: `docker compose -f dev.yml up -d --force-recreate oip`.

Between step 1 and step 3 logins fail - Keycloak rejects the old secret at the token endpoint. Existing sessions
keep working, because their tickets are already stored.

## Keycloak event shared secret

`KeycloakSync:SharedSecret` signs the `X-Keycloak-Signature` header of the event webhook. Its counterpart is the
`sharedSecret` field of the realm attribute `_providerConfig.ext-event-http.0`, which is what
[Keycloak User Synchronization](../ru/KeycloakUserSync.md) (in Russian) configures.

The attribute value is a JSON document and the attribute key contains dots, so `kcadm.sh -s key=value` cannot
address it. The attribute is rewritten as a whole: fetch the realm, replace `sharedSecret`, send it back - which is
what `apply-secrets.sh` does.

````shell
docker compose -f dev.yml exec -T keycloak /opt/keycloak/bin/kcadm.sh get realms/oip > realm.json
# replace sharedSecret in realm.json
docker compose -f dev.yml exec -T keycloak /opt/keycloak/bin/kcadm.sh update realms/oip -f - < realm.json
````

Keycloak applies the listener configuration immediately, so change the realm attribute and the application
configuration together and recreate the application containers. Events fired in between are rejected with `401` by
the receiving endpoint; the phasetwo listener retries them.

## Redis password

`REDIS_PASSWORD` is passed to `redis-server --requirepass` and substituted into the connection string the
applications use, so a single edit moves both sides. The password is not stored inside Redis, so no in-service
rotation step exists.

````shell
cd .oip-devcontainer
# edit REDIS_PASSWORD in .env
docker compose -f dev.yml up -d --force-recreate redis oip
````

Recreate Redis and the applications together. Authentication tickets are cache entries; restarting Redis logs
everyone out, it does not corrupt anything.

If only one side is changed, the applications keep serving requests - `DistributedAuthenticationTicketStore` falls
back to its in-memory store - and the startup log carries the reason:

````text
Redis rejected the configured credentials. Check the password in
'SecurityService:AuthTicketStore:RedisConnectionString' (REDIS_PASSWORD) against the credentials Redis was started
with.
````

That message comes from `RedisSecretConnectivityProbe`
(`Oip.Base/Security/Connectivity/`), a startup probe that opens one authenticated connection. A dependency that is
simply unreachable is logged as a warning; only rejected credentials are logged as an error.

## MinIO credentials

The `minio` container is started with a **root account** (`MINIO_ROOT_USER` / `MINIO_ROOT_PASSWORD`). That account
owns the instance: it can delete every bucket and create users. Applications should not authenticate with it.

Give the applications their own account instead. Set `MINIO_ACCESS_KEY` and `MINIO_SECRET_KEY` to values different
from the root ones and run `./apply-secrets.sh`, or do it by hand:

````shell
docker compose -f dev.yml exec -T minio mc alias set local http://localhost:9000 \
  "$MINIO_ROOT_USER" "$MINIO_ROOT_PASSWORD"

docker compose -f dev.yml exec -T minio mc admin user add local "$MINIO_ACCESS_KEY" "$MINIO_SECRET_KEY"
docker compose -f dev.yml exec -T minio mc admin policy attach local readwrite --user "$MINIO_ACCESS_KEY"
````

`readwrite` is the built-in policy and it covers every bucket. To scope the account to the two buckets the
applications actually use, create a policy first and attach that one instead:

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

`mc admin user add` on an existing account replaces its secret key, which is how the account's credentials are
rotated later. Recreate the application containers afterwards; MinIO signs each request separately, so no session
survives the change.

Rotating the root password is a separate step: change `MINIO_ROOT_PASSWORD` and recreate the `minio` container.
Accounts created under the old root account keep working, because they are stored in MinIO's own database.

A rejected key pair is reported at startup by the object storage probes registered in
`Oip.Users.Base` and `Oip.Discussions.Base`:

````text
MinIO (UserPhotoStorage) rejected the configured credentials. Check 'UserPhotoStorage:AccessKey' and
'UserPhotoStorage:SecretKey' (MINIO_ACCESS_KEY, MINIO_SECRET_KEY) against the credentials MinIO was started with.
````

## Development certificate

`DEV_CERT_PASSWORD` is the password of `https/oip.pfx`. It is set when the file is generated, by the
`openssl pkcs12 -export -passout pass:...` command in `.oip-devcontainer/README.md`. Regenerate the file with the
new password, then recreate the application containers.

## Outside the development container

The compose file and `.env` are the development container's mechanism. In any other installation the same
variables are supplied by the deployment: Kubernetes secrets, systemd unit `Environment=` lines, or the secret
store of the platform. The application side of every secret is an ASP.NET Core configuration key, so the
environment variable name is the key with `:` replaced by `__`:

| Configuration key | Environment variable |
| --- | --- |
| `SecurityService:ClientSecret` | `SecurityService__ClientSecret` |
| `SecurityService:AuthTicketStore:RedisConnectionString` | `SecurityService__AuthTicketStore__RedisConnectionString` |
| `KeycloakSync:SharedSecret` | `KeycloakSync__SharedSecret` |
| `UserPhotoStorage:AccessKey`, `UserPhotoStorage:SecretKey` | `UserPhotoStorage__AccessKey`, `UserPhotoStorage__SecretKey` |
| `DiscussionAttachmentStorage:AccessKey`, `DiscussionAttachmentStorage:SecretKey` | `DiscussionAttachmentStorage__AccessKey`, `DiscussionAttachmentStorage__SecretKey` |

During development `dotnet user-secrets` is the alternative that keeps values out of the working tree entirely.

## Checklist

After a rotation, confirm that:

* the startup log reports `Default secret validation passed` for every service - see
  [Default Secrets](./DefaultSecrets.md);
* the startup log carries no `rejected the configured credentials` line;
* `/health` reports the `default-secrets` check as healthy rather than degraded;
* a fresh login succeeds, which exercises the Keycloak client secret;
* uploading a user photo succeeds, which exercises the MinIO credentials.
