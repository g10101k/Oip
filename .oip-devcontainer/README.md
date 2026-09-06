# Development Container

## Secrets

Every secret has a default written into `dev.yml` as `${VAR:-default}`, so the stack starts without any further
setup. To use your own values, copy `.env.example` to `.env` and edit the copy: Docker Compose picks `.env` up
automatically and substitutes it into both the infrastructure containers and the application configuration. `.env`
is git-ignored, which is why the repository ships the example rather than the file itself.

````shell
cd .oip-devcontainer
cp .env.example .env
````

The `oip-backend` client secret, the event listener shared secret and the MinIO account the applications use are
stored by Keycloak and MinIO themselves rather than read from their environment, so recreating a container does not
move them. `./apply-secrets.sh` writes all three into the running stack from `.env` and is idempotent.

The values shipped with the repository are public. `doc/en/SecretRotation.md` (`doc/ru/SecretRotation.md`) explains,
per secret, what to change, in which order, and what has to be restarted.

## Certificate Generation

To generate CA certificates use:

````shell
cd .oip-devcontainer

mkdir -p https

openssl req -x509 -sha256 -days 365 -nodes -newkey rsa:2048 -keyout ./https/oip-dev-ca.key -out ./https/oip-dev-ca.crt -config oip-dev-ca.conf
````

To generate development certificates use:

````shell
cd .oip-devcontainer

openssl genrsa -out ./https/oip.key 2048
openssl req -new -key ./https/oip.key -out ./https/oip.csr -config oip.conf

openssl x509 -req -in ./https/oip.csr -CA ./https/oip-dev-ca.crt -CAkey ./https/oip-dev-ca.key -CAcreateserial -out ./https/oip.pem -days 365 -sha256 -extfile oip.conf -extensions req_ext
openssl pkcs12 -export -out ./https/oip.pfx -inkey ./https/oip.key -in ./https/oip.pem -passout pass:"${DEV_CERT_PASSWORD:-P@ssw0rd}"
````

## Development CA Trust

The app images trust `./https/oip-dev-ca.crt` during Docker build. Docker Compose passes only the `https` folder as a named
build context, and the Dockerfiles copy only the public CA certificate into the container trust store.

After regenerating `oip-dev-ca.crt`, rebuild the affected distributed service images:

````shell
docker compose -f dev.yml --profile distributed up --build --force-recreate -d oip-users oip-applications oip-notifications
````

For only the backend development services, you can also use:

````shell
./rebuild.sh
````

## Keycloak Events

The Keycloak development image is built from `./keycloak/Dockerfile` and installs
`io.phasetwo.keycloak:keycloak-events:0.61` for Keycloak `26.6.3`.

The imported `oip` realm enables the `ext-event-http` event listener for admin events and sends them to the app running on the host:

````text
https://host.docker.internal:5002/api/keycloak-events/receive-keycloak-event
````

The webhook is signed with `X-Keycloak-Signature` using the shared secret configured in both
`realm-export.json` and the app `KeycloakSync:SharedSecret` setting. Both sides come from
`KEYCLOAK_EVENTS_SHARED_SECRET`; `./apply-secrets.sh` pushes it into a realm that has already been imported.

If the Keycloak Postgres volume already contains the realm, changing `realm-export.json` will not update it
automatically. Update the realm attributes/listeners in the Admin UI or recreate the Keycloak database volume.

## Development Container Startup

Run the commands from the `.oip-devcontainer` directory:

````shell
cd .oip-devcontainer
````

Use the OS-specific override to persist ASP.NET Data Protection keys on the host.

### Standalone Mode

In standalone mode, run `Oip` with `IsStandalone=true`. The development compose file starts only shared infrastructure;
`oip-users`, `oip-applications`, and `oip-notifications` are not started as separate containers.

### Unix, macOS, Linux

````shell
docker compose -f dev.yml -f dev.unix.yml up -d
````

The Unix override mounts `${HOME}/.aspnet/DataProtection-Keys` into `/PersistKeys` inside the app containers.

### Windows

````powershell
docker compose -f dev.yml -f dev.windows.yml up -d
````

The Windows override mounts `${LOCALAPPDATA}/ASP.NET/DataProtection-Keys` into `/PersistKeys` inside the app containers.

### Without Host Data Protection Keys

````shell
docker compose -f dev.yml up -d
````

This starts the development services without mounting the host Data Protection keys folder.

### Distributed Mode

In distributed mode, run `Oip` with `IsStandalone=false` and start the distributed profile:

````shell
docker compose -f dev.yml -f dev.unix.yml --profile distributed up -d
````

On Windows:

````powershell
docker compose -f dev.yml -f dev.windows.yml --profile distributed up -d
````

The override mounts the host Data Protection keys folder into `/PersistKeys` inside `oip-users`,
`oip-applications`, and `oip-notifications`. The base `dev.yml` points
`DataProtection__PersistKeysToFileSystemPath` to that path.

## Test Container Startup

The UI-test container compose file has moved to `../.oip-testcontainer/docker-compose.yml`. Run
it from the `.oip-testcontainer` directory:

````shell
cd ../.oip-testcontainer
docker compose up --build --force-recreate
````
