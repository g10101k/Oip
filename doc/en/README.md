# OIP
A foundational set of concepts for developing cross-platform web applications based on the following stack:
* Angular 19 (prime-ng, sakai-ng);
* .NET 8.0 (EFCore);
* KeyCloak.

The core value of this project lies in the development approaches used in this example.

# Development
First-time setup:
1. Install .NET 8.0 SDK: https://dotnet.microsoft.com/en-us/download/dotnet/8.0;
2. Install the latest LTS version of Node.js: https://nodejs.org/en;
3. Install Docker Desktop: https://www.docker.com/get-started;
4. Start PostgreSQL: docker compose -f docker-compose-common.yml up postgres -d;
5. Run the `Oip` project—after launch, certificates for the current site and Keycloak should be generated;
6. Start KeyCloak:
   * For Windows: `docker compose -f docker-compose-common.yml -f docker-compose-windows.yml up keycloak -d`;
   * For Linux/Mac: `docker compose -f docker-compose-common.yml -f docker-compose-macos-linux.yml up keycloak -d`;
7. In KeyCloak, add a user with the realm role `admin` to the `oip` realm. Use the credentials `admin` / `P@ssw0rd` to log in;
8. You can now log in to the portal with this user.

Subsequent runs can be performed using:
* `.devcontainer/run-unix.sh`
* `.devcontainer/run-windows.cmd`

# Concepts

* [Modules](./Modules.md)
* [Swagger WebClient Generator](./SwaggerWebClientGenerator.md)


# Known Issues

1. **Using Tokens in SPA** - Token leakage is possible when using iframes.
2. **Permission Checks on Resource Server** - Direct resource access via URL may be possible.
3. **Storing Tokens in LocalStorage** - An insecure storage method, making the token vulnerable to XSS attacks.

> 🔐 **Risks**: Potential token leaks, suboptimal authorization scheme, vulnerability to hacking.  
