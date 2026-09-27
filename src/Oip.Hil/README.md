# Oip.Hil — Human-in-the-loop workflows

* Dotnet app start at https://localhost:5009
* Angular client (`oip-hil`) start at https://localhost:50009

Hosts example Temporal workflows that wait for users (`Workflows/`) and the demo controller that starts them.
The shared infrastructure — Temporal worker, user steps, step pages and workflow activity module controllers — lives
in `Oip.Hil.Base`.

## Services

Applications, users, discussions and notifications are added according to `ServiceAddingMode`: `Remote` (default)
uses the separate services from `Services`, `Local` hosts them in the application with their controllers.
`Services:Shell` points to the application itself, so the Angular dev server proxies everything except the remote
services to it.

## Temporal

Connection is configured in the `Temporal` section of `appsettings.json` (address, namespace, task queue, API key,
TLS). A local server is started by the `temporal` service of `.oip-devcontainer/dev.yml` (UI at http://localhost:8233).

## Web client generation

```shell
dotnet run --project Oip.Hil.csproj --no-restore -- --GenerateWebClient=true
```

The client is generated into `Oip.WebClient/projects/oip-hil/src/api`.
