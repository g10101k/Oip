#!/usr/bin/env bash
#
# Pushes the secrets from `.env` into the running Keycloak and MinIO containers.
#
# Compose substitutes `.env` into `dev.yml` on every `up`, so container passwords and application
# configuration follow the file on their own. Three secrets do not, because the services store them
# themselves: the `oip-backend` client secret and the event listener shared secret live in the
# Keycloak database (`keycloak/realm-export.json` is read only on the first start of an empty
# database), and the MinIO account the applications authenticate with lives in MinIO's own store on
# the `minio_data` volume. This script writes all three, and is idempotent: running it twice leaves
# the same state.
#
# Usage, from the `.oip-devcontainer` directory, with the stack already running:
#
#   ./apply-secrets.sh
#
# See `doc/en/SecretRotation.md` for what has to be restarted afterwards.

set -euo pipefail

cd "$(dirname "$0")"

if [ -f .env ]; then
  set -a
  # shellcheck disable=SC1091
  . ./.env
  set +a
fi

COMPOSE=${COMPOSE:-docker compose -f dev.yml}

REALM=${KEYCLOAK_REALM:-oip}
CLIENT_ID=${KEYCLOAK_BACKEND_CLIENT_ID:-oip-backend}
KEYCLOAK_ADMIN_USERNAME=${KEYCLOAK_ADMIN_USERNAME:-admin}
KEYCLOAK_ADMIN_PASSWORD=${KEYCLOAK_ADMIN_PASSWORD:-P@ssw0rd}
KEYCLOAK_CLIENT_SECRET=${KEYCLOAK_CLIENT_SECRET:-Xnw6h8wH0dgehxGxZYgWKbKosKCx9E8L}
KEYCLOAK_EVENTS_SHARED_SECRET=${KEYCLOAK_EVENTS_SHARED_SECRET:-change-me-keycloak-events}
LISTENER_ATTRIBUTE=_providerConfig.ext-event-http.0
MINIO_ROOT_USER=${MINIO_ROOT_USER:-admin}
MINIO_ROOT_PASSWORD=${MINIO_ROOT_PASSWORD:-P@ssw0rd}
MINIO_ACCESS_KEY=${MINIO_ACCESS_KEY:-admin}
MINIO_SECRET_KEY=${MINIO_SECRET_KEY:-P@ssw0rd}
MINIO_POLICY=${MINIO_POLICY:-readwrite}

kc() {
  $COMPOSE exec -T keycloak /opt/keycloak/bin/kcadm.sh "$@"
}

echo "==> Keycloak: authenticating as ${KEYCLOAK_ADMIN_USERNAME}"
kc config credentials \
  --server http://localhost:8080 \
  --realm master \
  --user "$KEYCLOAK_ADMIN_USERNAME" \
  --password "$KEYCLOAK_ADMIN_PASSWORD" >/dev/null

echo "==> Keycloak: setting the '${CLIENT_ID}' client secret"
CLIENT_UUID=$(kc get clients -r "$REALM" -q "clientId=${CLIENT_ID}" --fields id --format csv --noquotes | tr -d '\r\n')
if [ -z "$CLIENT_UUID" ]; then
  echo "Client '${CLIENT_ID}' not found in realm '${REALM}'." >&2
  exit 1
fi
kc update "clients/${CLIENT_UUID}" -r "$REALM" -s "secret=${KEYCLOAK_CLIENT_SECRET}"

echo "==> Keycloak: setting the '${LISTENER_ATTRIBUTE}' shared secret"
# The listener config is a JSON document stored as a realm attribute, so it is rewritten as a whole:
# kcadm's `-s key=value` cannot address a key that itself contains dots.
REALM_JSON=$(mktemp)
trap 'rm -f "$REALM_JSON"' EXIT
kc get "realms/${REALM}" > "$REALM_JSON"
if grep -q 'sharedSecret' "$REALM_JSON"; then
  sed 's/\\"sharedSecret\\":\\"[^\\]*\\"/\\"sharedSecret\\":\\"'"${KEYCLOAK_EVENTS_SHARED_SECRET}"'\\"/' \
    "$REALM_JSON" > "${REALM_JSON}.new"
  kc update "realms/${REALM}" -f - < "${REALM_JSON}.new"
  rm -f "${REALM_JSON}.new"
else
  echo "    Realm '${REALM}' has no '${LISTENER_ATTRIBUTE}' listener configured, nothing to update."
  echo "    See doc/ru/KeycloakUserSync.md for how to add it."
fi

if [ "$MINIO_ACCESS_KEY" = "$MINIO_ROOT_USER" ]; then
  echo "==> MinIO: skipped, MINIO_ACCESS_KEY equals MINIO_ROOT_USER"
  echo "    The applications authenticate as the MinIO root account. Give MINIO_ACCESS_KEY and"
  echo "    MINIO_SECRET_KEY dedicated values and re-run this script to create a separate account."
else
  echo "==> MinIO: creating the '${MINIO_ACCESS_KEY}' account with the '${MINIO_POLICY}' policy"
  $COMPOSE exec -T minio sh -s -- \
    "$MINIO_ROOT_USER" "$MINIO_ROOT_PASSWORD" "$MINIO_ACCESS_KEY" "$MINIO_SECRET_KEY" "$MINIO_POLICY" <<'MC'
set -e
mc alias set local http://localhost:9000 "$1" "$2" >/dev/null
mc admin user add local "$3" "$4" >/dev/null
mc admin policy attach local "$5" --user "$3" >/dev/null 2>&1 || true
MC
fi

echo
echo "Done. Recreate the application containers so that they pick up the new configuration:"
echo "  ${COMPOSE} up -d --force-recreate oip"
