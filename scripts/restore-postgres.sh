#!/bin/bash
# Restores a Postgres backup produced by scripts/backup-postgres.sh — see
# docs/DEPLOYMENT_ARCHITECTURE.md's Backup strategy section.
#
# Usage: scripts/restore-postgres.sh <path-to-backup.sql.gz>
#
# This is destructive to the target database — it drops and recreates the schema
# before restoring. Confirms before proceeding unless FORCE=1 is set.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
BACKUP_FILE="${1:-}"
CONTAINER_NAME="${POSTGRES_CONTAINER:-deploy-postgres-1}"

if [ -z "$BACKUP_FILE" ] || [ ! -f "$BACKUP_FILE" ]; then
  echo "Usage: $0 <path-to-backup.sql.gz>" >&2
  exit 1
fi

ENV_FILE="$SCRIPT_DIR/../deploy/.env"
POSTGRES_DB="smartcampus"
POSTGRES_USER="smartcampus"
if [ -f "$ENV_FILE" ]; then
  POSTGRES_DB="$(grep -E '^POSTGRES_DB=' "$ENV_FILE" | cut -d= -f2- || echo "$POSTGRES_DB")"
  POSTGRES_USER="$(grep -E '^POSTGRES_USER=' "$ENV_FILE" | cut -d= -f2- || echo "$POSTGRES_USER")"
fi

if [ "${FORCE:-0}" != "1" ]; then
  echo "This will DROP and recreate all data in database '$POSTGRES_DB' on container '$CONTAINER_NAME'."
  read -r -p "Type the database name to confirm: " confirmation
  if [ "$confirmation" != "$POSTGRES_DB" ]; then
    echo "Aborted — confirmation did not match." >&2
    exit 1
  fi
fi

echo "==> Stopping the api container so nothing writes during restore"
docker compose -f "$SCRIPT_DIR/../deploy/docker-compose.yml" stop api 2>/dev/null || true

echo "==> Dropping and recreating the '$POSTGRES_DB' schema"
docker exec "$CONTAINER_NAME" psql -U "$POSTGRES_USER" -d "$POSTGRES_DB" \
  -c "DROP SCHEMA public CASCADE; CREATE SCHEMA public;"

echo "==> Restoring from $BACKUP_FILE"
gunzip -c "$BACKUP_FILE" | docker exec -i "$CONTAINER_NAME" psql -U "$POSTGRES_USER" -d "$POSTGRES_DB"

echo "==> Restarting the api container"
docker compose -f "$SCRIPT_DIR/../deploy/docker-compose.yml" up -d api

echo "==> Restore complete."
