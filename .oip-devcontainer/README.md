# Development Container

## Certificate Generation

To generate certificates use:

````shell
cd .oip-devcontainer

mkdir -p https

openssl req -x509 -sha256 -days 365 -nodes -newkey rsa:2048 -keyout ./https/oip-dev-ca.key -out ./https/oip-dev-ca.crt -config oip-dev-ca.conf

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

To start dev containers use:

````shell
docker compose -f test.yml up --build --force-recreate
````
