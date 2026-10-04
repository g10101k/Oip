# UI test container

**English** | [Русский](README.ru.md)

Brings up the Oip application (services moved from `../.oip-devcontainer/test.yml`) and Selenium Grid (hub + Chrome node)
for running `Oip.UiTest`. Certificates and credentials are the existing Oip dev stubs
(`../.oip-devcontainer/https`); no secrets from other projects have been brought in.

## Starting the containers

```shell
docker compose up -d --build --force-recreate
```

Without rebuilding:

```shell
docker compose up -d
```

## Running UI tests from the console

```shell
dotnet test ./../src/Oip.UiTest/Oip.UiTest.csproj --settings ./settings/default.runsettings
```

By default (without `--settings`), `TestSetup` starts a local `ChromeDriver` and targets
`https://localhost:50000` — so the tests can run without Docker at all if the application is already
running locally. `--settings ./settings/default.runsettings` switches the tests to the Selenium Grid
from this docker-compose (`RemoteDriverUrl`) and to the address of the `oip` service inside the docker network (`BaseUrl`).

## Test users

The `oip` realm (`../.oip-devcontainer/keycloak/realm-export.json`) contains two users, both with the `P@ssw0rd`
password:

| User    | Realm role | Run parameters                 |
|---------|------------|--------------------------------|
| `admin` | `admin`    | `Username`, `Password`         |
| `user`  | `user`     | `UserUsername`, `UserPassword` |

The tests sign in as `admin`; `user` is used to check what a non-administrator can access. Keycloak imports the realm
only into an empty database, so on an existing dev container volume the `user` account has to be created manually or
the Keycloak database volume recreated.

## Object storage and uploaded files

User photos are kept in MinIO (the `minio` service); the application creates the bucket on the first upload. The
`UserProfileTests` upload `src/Oip.UiTest/TestData/avatar.png`: the browser takes the file from `BaseDirectory`, which
is the test output folder for a local run and `/home/seluser/uploads` — the `src/Oip.UiTest` folder mounted into the
Chrome node — for the Selenium Grid.

## Servers started by the tests

`ModuleRegistryTests` register an extension module from a manifest served by the test process itself; the backend
downloads it from `http://<TestHost>:<random port>/manifest.json`. `TestHost` is `localhost` by default and
`host.docker.internal` in `default.runsettings`, which the `oip` container resolves to the host. `SessionsTests` open a
second browser, so the Chrome node allows two sessions (`SE_NODE_MAX_SESSIONS`).

## Running out of disk space

```shell
docker builder prune -af
```

## Viewing test recordings

Test run videos are stored in an anonymous `/videos` volume of the `chrome-video` container and are available via
`file-browser` at `http://localhost:8081`. `docker compose down -v` removes them together with the containers.
