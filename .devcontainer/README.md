# Development Container

## Certificate Generation

To generate certificates use:

````shell
dotnet dev-certs https -ep ./https/oip.pfx -p P@ssw0rd
dotnet dev-certs https -ep ./https/oip.pem --format Pem --no-password 
dotnet dev-certs https --trust
````

## Development Container Startup

To start dev containers use:

````shell
docker compose up keycloak --build --force-recreate
````

## Test Container Startup

````shell
docker compose up --build --force-recreate
````