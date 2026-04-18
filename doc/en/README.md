# OIP
A foundational set of concepts for developing cross-platform web applications based on the following stack:
* Angular 20 (prime-ng, sakai-ng);
* .NET 8.0 (EFCore);
* KeyCloak.

The core value of this project lies in the development approaches used in this example.

# Development
First-time setup:
1. Install .NET 8.0 SDK: https://dotnet.microsoft.com/en-us/download/dotnet/8.0;
2. Install the latest LTS version of Node.js: https://nodejs.org/en;
3. Install Docker Desktop: https://www.docker.com/get-started;
4. Go to the `.devcontainer` directory;
5. Generate certificates if needed, following `.devcontainer/README.md`;
6. Start the development infrastructure: `docker compose -f dev.yml up -d`;
7. In KeyCloak, add a user with the realm role `admin` to the `oip` realm. Use the credentials `admin` / `P@ssw0rd` to log in;
8. You can now log in to the portal with this user.

Subsequent runs can be performed using:
* `cd .devcontainer`
* `docker compose -f dev.yml up -d`

# Concepts

* [Modules](./Modules.md)
* [Swagger WebClient Generator](./SwaggerWebClientGenerator.md)


# Known Issues

1. **Using Tokens in SPA** - Token leakage is possible when using iframes.
2. **Permission Checks on Resource Server** - Direct resource access via URL may be possible.
3. **Storing Tokens in LocalStorage** - An insecure storage method, making the token vulnerable to XSS attacks.

> 🔐 **Risks**: Potential token leaks, suboptimal authorization scheme, vulnerability to hacking.  
