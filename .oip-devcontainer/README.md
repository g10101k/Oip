# Development Container

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
openssl pkcs12 -export -out ./https/oip.pfx -inkey ./https/oip.key -in ./https/oip.pem -passout pass:P@ssw0rd
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
`realm-export.json` and the app `KeycloakSync:SharedSecret` setting.

If the Keycloak Postgres volume already contains the realm, changing `realm-export.json` will not update it
automatically. Update the realm attributes/listeners in the Admin UI or recreate the Keycloak database volume.

## Temporal

The `temporal` service runs the Temporal development server (`temporal server start-dev`) with SQLite persisted in the
`temporal_data` volume.

- gRPC frontend: `localhost:7233` (`temporal:7233` from other containers), namespace `default`
- Web UI: http://localhost:8233

## Open WebUI

The `open-webui` service is the chat UI of the agent gateway of `Oip.Hitl`: http://localhost:3000.

- Sign-in goes through Keycloak (client `open-webui` of the `oip` realm); the first user to sign in becomes the Open WebUI
  admin.
- The agents are the models of the OIP pipe (see below), which calls `Oip.Hitl` running on the host
  (`OIP_GATEWAY_URL=https://host.docker.internal:5009/v1`) with the access token of the signed-in user, so start
  `Oip.Hitl` to chat. There is no OpenAI connection (`ENABLE_OPENAI_API=false`): until the pipe is imported, no models
  are listed.
- The configuration lives in `dev.yml` (`ENABLE_PERSISTENT_CONFIG=false`): changes made in the admin panel are lost on
  restart.

Keycloak runs with `KC_HOSTNAME=https://localhost:8443` and `KC_HOSTNAME_BACKCHANNEL_DYNAMIC=true`: the issuer and the
browser URLs are always `localhost`, while containers reach the token, userinfo and JWKS endpoints at `keycloak:8443`.

The `open-webui` client is created only with a new Keycloak database. For an existing one, create it from
`realm-export.json`:

````shell
python3 -c "import json;c=[c for c in json.load(open('keycloak/realm-export.json'))['clients'] if c['clientId']=='open-webui'][0];c.pop('id');print(json.dumps(c))" > /tmp/open-webui-client.json
docker cp /tmp/open-webui-client.json oip-devcontainer-keycloak-1:/tmp/open-webui-client.json
docker exec oip-devcontainer-keycloak-1 bash -c '/opt/keycloak/bin/kcadm.sh config credentials --server http://localhost:8080 --realm master --user "$KC_BOOTSTRAP_ADMIN_USERNAME" --password "$KC_BOOTSTRAP_ADMIN_PASSWORD" && /opt/keycloak/bin/kcadm.sh create clients -r oip -f /tmp/open-webui-client.json'
````

If the client was created before its service account was enabled (needed by the OIP pipe below), enable it:

````shell
docker exec oip-devcontainer-keycloak-1 bash -c 'K=/opt/keycloak/bin/kcadm.sh; $K config credentials --server http://localhost:8080 --realm master --user "$KC_BOOTSTRAP_ADMIN_USERNAME" --password "$KC_BOOTSTRAP_ADMIN_PASSWORD" && $K update clients/$($K get clients -r oip -q clientId=open-webui --fields id --format csv --noquotes) -r oip -s serviceAccountsEnabled=true'
````

Tools of agents call services with the rights of the user (see `src/Oip.Hitl/README.md`): the gateway exchanges the
token of the user for a token of `oip-backend`. This needs the `oip-backend audience` mapper of `open-webui` and the
standard token exchange of `oip-backend` with refresh tokens in the same session. For an existing Keycloak database:

````shell
docker exec oip-devcontainer-keycloak-1 bash -c 'K=/opt/keycloak/bin/kcadm.sh; $K config credentials --server http://localhost:8080 --realm master --user "$KC_BOOTSTRAP_ADMIN_USERNAME" --password "$KC_BOOTSTRAP_ADMIN_PASSWORD" && $K create clients/$($K get clients -r oip -q clientId=open-webui --fields id --format csv --noquotes)/protocol-mappers/models -r oip -b "{\"name\":\"oip-backend audience\",\"protocol\":\"openid-connect\",\"protocolMapper\":\"oidc-audience-mapper\",\"config\":{\"included.client.audience\":\"oip-backend\",\"id.token.claim\":\"false\",\"access.token.claim\":\"true\",\"introspection.token.claim\":\"true\"}}" && $K update clients/$($K get clients -r oip -q clientId=oip-backend --fields id --format csv --noquotes) -r oip -b "{\"attributes\":{\"standard.token.exchange.enabled\":\"true\",\"standard.token.exchange.enableRefreshRequestedTokenType\":\"SAME_SESSION\"}}"'
````

### OIP pipe

The pipe `src/Oip.Hitl/OpenWebUi/oip_agents_pipe.py` adds the agents as models named `OIP: <agent>`. Unlike a plain
OpenAI connection, it shows tool calls as statuses, and questions of the agent and approvals of tool calls as dialogs.
Open WebUI keeps functions in its database, so import the pipe once: Admin Panel → Functions → Import (or `+` and
paste the file), then enable it. It takes the gateway URL and the Keycloak client from the container environment; the
valves override them.

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
