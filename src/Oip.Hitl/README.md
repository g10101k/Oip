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
server-sent events. Closing the request (the Stop button) cancels the workflow. Models are the enabled agents,
identified by their code. Routes and errors follow the OpenAI API, so the controller is excluded from the web client.

Requests are authenticated with a Keycloak access token of the `oip` realm (`Authorization: Bearer ...`). The
`open-webui` service of `.oip-devcontainer/dev.yml` is connected to the gateway and forwards the token of the user signed
in through Keycloak: start the dev container and `Oip.Hitl`, then open http://localhost:3000.

## Agents, skills and tools

- **Agent** (`/api/agent-module`, table `llm.Agent`): a model of the gateway — system prompt, LLM provider (the default
  one when empty) and the skills it may load.
- **Skill** (`llm.Skill`): a description the model always sees, instructions it gets on loading the skill with the
  `load_skill` tool, and the tools the skill gives it.
- **Tool**: a Temporal activity marked with `[AgentTool("name")]` (see `DemoToolActivities`). It takes no parameters or
  one class of the arguments; the JSON schema shown to the model is generated from it, so name the properties with
  `JsonPropertyName` and describe them with `Description`. Tools of the registered activities form the tool catalog
  (`get-tools`); skills refer to them by name.

`AgentWorkflow` runs the loop: a model turn (`ChatTurn`) is offered `load_skill` and the tools of the loaded skills,
the workflow makes the tool calls as activities and sends the results back until the model answers without calling
tools (at most 16 turns; the last one is offered no tools). A failed tool call is returned to the model as an error
text. Tool calls are streamed to the chat as reasoning (`🔧 name {arguments}`).

```shell
curl -N https://localhost:5009/v1/chat/completions -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"model":"<agent code>","stream":true,"messages":[{"role":"user","content":"Hi"}]}'
```
