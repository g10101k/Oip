# Oip

I'm trying to make a micro frontend platform based on angular + dotnet

For example:
https://github.com/angular-architects/module-federation-plugin/blob/main/libs/mf/tutorial/tutorial.md#part-1-clone-and-inspect-the-starter-kit

https://www.angulararchitects.io/blog/dynamic-module-federation-with-angular/

https://volosoft.com/blog/how-to-configure-angular-modules-loaded-by-the-router

# Develop

1. Install .NET 8.0 SDK https://dotnet.microsoft.com/en-us/download/dotnet/8.0
2. Install latest LTS node.js https://nodejs.org/en
3. Install Docker Desktop https://www.docker.com/get-started

```` shell
git clone https://github.com/g10101k/Oip.git
cd .devcontainer
docker compose up
````

Now you can start up Oip.csproj

# Security

Use keycloak or other openid security

## Known issues

1) Using a token in SPA. This may result in token leakage when using iframe;
2) Access to resources is checked on the resource server;
3) Store token in localstorage