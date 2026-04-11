# Application Modes: standalone and distributed

The application is designed to support two architectural modes:

- in `standalone`, the main backend modules run inside one primary application.
- in `distributed`, different services without a frontend run as separate applications.

Important: the frontend does not know how many services are running. It always works with a base URL. In `distributed`
mode, a proxy is configured for different URLs, while in `standalone` mode it points to a single URL. Because of this,
the proxy settings are built dynamically.

## Standalone mode

In `standalone` mode, the main application hosts the related modules locally inside itself. In this mode:

- the main backend remains the primary entry point
- auxiliary services `Oip.Users`, `Oip.Discussions`, and `Oip.Notifications` run inside the application

## Distributed mode

In `distributed` mode, the system is treated as a set of separate services. In this mode:

- the main backend remains the central entry point for part of the API
- individual domains can live in their own services

## How the mode is defined on the backend

The key configuration flag is `IsStandalone`. It is used as the architectural switch.

In addition to `IsStandalone`, the backend uses a list of service addresses to generate `proxy.conf.js`:

- `Oip` -> `https://localhost:5002`
- `OipUsers` -> `https://localhost:5005`
- `OipDiscussions` -> `https://localhost:5006`
- `OipNotifications` -> `https://localhost:5007`

## How the mode affects backend application composition

In the main application, the mode affects how the runtime backend configuration (DI) is actually built. This can be
seen in `Program.cs`:

- if `settings.IsStandalone == true`, the main application wires up `Oip.Users`, `Oip.Discussions`, and
  `Oip.Notifications` locally
- if `settings.IsStandalone == false`, the application runs in a more distributed setup where part of the
  functionality is expected to be provided by external services

## How the frontend detects the current mode

The frontend does not read `IsStandalone` directly. Instead, before the dev server starts, it runs the script:

- `./src/Oip.WebClient/scripts/generate-proxy-config.js`

It fetches settings from the backend endpoint `GET /api/proxy-settings/get-spa-proxy-settings` as target addresses:

- `main`
- `users`
- `discussion`
- `notification`

The response logic is:

- `main` is always equal to `appSettings.Services.Oip`
- in `standalone`, `users`, `discussion`, and `notification` also resolve to `appSettings.Services.Oip`
- in `distributed`, these addresses are taken from separate values in `Services`

After the frontend receives the settings, it compares the target addresses:

- if all addresses are equal, it treats the mode as `standalone`
- if the addresses differ, it treats the mode as `distributed`

Then it automatically generates the file `./src/Oip.WebClient/obj/proxy.generated.json`, and
`./src/Oip.WebClient/proxy.conf.js` reads the generated result.

## Resulting proxy routes

In `standalone`, all main routes go to the main backend.

In `distributed`, the routes are split across services:

- `/api/users`, `/api/user-profile` -> users service
- `/hubs/notification` -> notifications service
- `/api/discussion` -> discussions service
- `/api`, `/swagger`, `/health`, `/metrics` -> main service

## What happens during a normal frontend startup

In `package.json`, the following steps run automatically before `start`:

1. HTTPS certificate preparation
2. proxy configuration generation, which synchronizes with the backend environment that already defines the
   architectural mode of the application. If the frontend cannot fetch settings from the backend, it creates a
   fallback configuration in `distributed` mode and uses these servers:
    - `main` -> `https://localhost:5002`
    - `users` -> `https://localhost:5005`
    - `discussion` -> `https://localhost:5006`
    - `notification` -> `https://localhost:5007`
