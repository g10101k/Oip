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

## Running out of disk space

```shell
docker builder prune -af
```

## Viewing test recordings

Test run videos are stored in an anonymous `/videos` volume of the `chrome-video` container and are available via
`file-browser` at `http://localhost:8081`. `docker compose down -v` removes them together with the containers.
