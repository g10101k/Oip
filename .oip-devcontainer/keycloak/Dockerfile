ARG KEYCLOAK_VERSION=26.6.3

FROM quay.io/keycloak/keycloak:${KEYCLOAK_VERSION}

ARG KEYCLOAK_EVENTS_VERSION=0.61

ADD --chown=keycloak:keycloak \
    https://repo1.maven.org/maven2/io/phasetwo/keycloak/keycloak-events/${KEYCLOAK_EVENTS_VERSION}/keycloak-events-${KEYCLOAK_EVENTS_VERSION}.jar \
    /opt/keycloak/providers/keycloak-events.jar
