# OIP

A foundational set of concepts for developing cross-platform web applications based on the following stack:

* Angular 20 (prime-ng, sakai-ng);
* .NET 8.0 (EFCore);
* Keycloak.

The core value of this project lies in the approaches used in the development of this example.

# Development

First-time setup:

1. Install the .NET 8.0 SDK https://dotnet.microsoft.com/en-us/download/dotnet/8.0;
2. Install the latest LTS version of Node.js https://nodejs.org/en;
3. Install Docker Desktop https://www.docker.com/get-started;
4. Go to the `.oip-devcontainer` directory;
5. If needed, generate certificates following the instructions in `.oip-devcontainer/README.md`;
6. Start the development infrastructure with `docker compose -f dev.yml up -d`;
7. Use the login `admin` / `P@ssw0rd`;
8. You can now log in to the portal with this user;

Subsequent runs can be performed using:

* `cd .devcontainer`
* `docker compose -f dev.yml up -d`

# Concepts

* [Modules](./Modules.md)
* [Localization](./L10n.md)
* [Controller Registration](./ControllerRegistration.md)
* [Web API Client Generation](./SwaggerWebClientGenerator.md)
* [Standalone & Distributed Mode](./StandaloneAndDistributedMode.md)
* [Security](./Security.md)
* [Keycloak User Synchronization](./KeycloakUserSync.md)
* [Frontend Theme Management](./ThemeManagement.md)

# Known Issues

1. **Permission Checks on the Resource Server** - direct access via a resource link may be possible.
