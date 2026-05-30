# Development Container

## Certificate Generation

To generate certificates use:

````shell
cd .oip-devcontainer

mkdir -p https

openssl req -x509 -sha256 -days 3560 -nodes -newkey rsa:2048 -subj "/CN=developer-ca/C=oi/L=oip" -keyout ./https/dev-ca.key -out ./https/dev-ca.crt 

openssl genrsa -out ./https/oip.key 2048
openssl req -new -key ./https/oip.key -out ./https/oip.csr -config oip.conf

openssl x509 -req -in ./https/oip.csr -CA ./https/dev-ca.crt -CAkey ./https/dev-ca.key -CAcreateserial -out ./https/oip.pem -days 3650 -sha256 -extfile oip.conf -extensions req_ext
openssl pkcs12 -export -out ./https/oip.pfx -inkey ./https/oip.key -in ./https/oip.pem -passout pass:P@ssw0rd
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
