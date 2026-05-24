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
docker compose -f dev.yml up -d
````

## Test Container Startup

To start dev containers use:

````shell
docker compose -f test.yml up --build --force-recreate
````