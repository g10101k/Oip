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
`open-webui` service of `.oip-devcontainer/dev.yml` reaches the gateway through the OIP pipe, which forwards the token of
the user signed in through Keycloak: start the dev container and `Oip.Hitl`, import the pipe once, then open
http://localhost:3000.

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

### Human in the loop

`AgentWorkflow` is a user workflow, so the agent waits for the user in user steps:

- the built-in `ask_user` tool asks a question, optionally with answers to choose from;
- a tool marked with `[AgentTool("name", RequiresApproval = true)]` (e.g. `send_notification`) is called only after
  the user allows the call; a denied call, with the comment of the user, is returned to the model as an error text.

A step not completed within `AgentGateway:UserStepTimeoutMinutes` (10) is reported to the model as not answered; the
whole run, waiting included, is limited by `AgentGateway:RunTimeoutMinutes` (30). Steps of agent runs are in the task
list and are completed on the `workflow-task` page like other steps, or in the chat:

- The Open WebUI pipe `OpenWebUi/oip_agents_pipe.py` (see `.oip-devcontainer/README.md`) sends `"oip_events": true`:
  statuses and steps come as `oip` events of the stream (`{"type":"user_step","run_id":...,"step_id":...,"kind":
  "question"|"approval",...}`), which the pipe shows as statuses and dialogs, and the answer is sent to
  `POST /v1/agent-runs/{runId}/steps/{stepId}/complete` with `{"result":"...","comment":"..."}`. Only the user who
  started the run completes its steps there.
- Other OpenAI clients get the step as a quote in the answer with the link to its `workflow-task` page; the stream
  goes on once the step is completed there.

### User rights and skill workers

Tools call services with the rights of the user who chats, not with a service account:

1. When a run starts, the gateway exchanges the access token of the user (Keycloak standard token exchange) for a
   refresh token of `oip-backend` in the same Keycloak session. It is kept in Redis, protected with Data Protection,
   until the run ends; it is never in the workflow input or history.
2. A tool gets an access token with `IAgentUserTokenProvider.GetAccessTokenAsync(audience?)`: Oip.Hitl refreshes it
   from the refresh token and, with an audience, narrows it to that client. The OIP API checks the rights of the user
   as for the browser; when the user signs out of Keycloak, the tools get no token any more.

Keycloak setup: the `open-webui` client puts `oip-backend` into the audience of its tokens, and `oip-backend` allows
the standard token exchange with refresh tokens in the same session (see `.oip-devcontainer/README.md`). When the
exchange fails, e.g. for a token of another client, the chat works, and tools that need the user token fail with an
error the model reports.

`OipDataToolActivities` are demo tools of this kind: `get_my_notifications` and `search_users` (administrators only)
call the users and notifications API with the token of the user — Oip.Hitl itself in the Local mode
(`Application:InternalBaseUrl`), otherwise `Services:UsersService` and `Services:NotificationsService`.

Tools may also be hosted by a separate **skill worker** on its own task queue: it registers with
`AddAgentSkillWorker` (`Oip.Hitl.Base.Agents`), which on start sends its `[AgentTool]` activities to Oip.Hitl (gRPC
`GrpcAgentService.RegisterTools`, table `llm.RegisteredTool`), and its tools get the user token through
`GrpcAgentService.GetUserToken`. The gRPC service is called with the service account token (`ServiceAccountPolicy`) and
gives the token only for a running agent run.
