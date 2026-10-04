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

## Agent gateway (OpenAI-compatible API)

`AgentGatewayController` exposes `GET /v1/models` and `POST /v1/chat/completions` for chat UIs such as Open WebUI.
Each chat message starts an `AgentWorkflow`; with `stream: true` the activity publishes the answer to a Redis stream
(`AgentGateway:RedisConnectionString`, the auth ticket store Redis by default) and the gateway forwards it as
server-sent events. Closing the request (the Stop button) cancels the workflow. Models are the enabled LLM providers,
identified by name. Routes and errors follow the OpenAI API, so the controller is excluded from the web client.

Requests are authenticated with a Keycloak access token of the `oip` realm (`Authorization: Bearer ...`). The
`open-webui` service of `.oip-devcontainer/dev.yml` is connected to the gateway and forwards the token of the user signed
in through Keycloak: start the dev container and `Oip.Hitl`, then open http://localhost:3000.

```shell
curl -N https://localhost:5009/v1/chat/completions -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"model":"<provider name>","stream":true,"messages":[{"role":"user","content":"Hi"}]}'
```
