#!/bin/bash
# Nightly Postgres backup for a single school's deployment — see
# docs/DEPLOYMENT_ARCHITECTURE.md's Backup strategy section.
#
# Usage: scripts/backup-postgres.sh [backup-dir]
#   Defaults to ./backups relative to this script. Intended to run via host cron,
#   e.g.: 0 2 * * * /path/to/deploy/scripts/backup-postgres.sh /var/backups/smartcampus
#
# Rotation: keeps the last 14 daily backups; anything older is deleted. For the
# 6-monthly retention mentioned in docs/DEPLOYMENT_ARCHITECTURE.md, point a
# separate off-box sync (rsync/rclone to the agency's backup storage) at this
# same backup directory — that's a deliberately separate concern from this script.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
BACKUP_DIR="${1:-$SCRIPT_DIR/../backups}"
CONTAINER_NAME="${POSTGRES_CONTAINER:-deploy-postgres-1}"
RETENTION_DAYS=14

mkdir -p "$BACKUP_DIR"

# Read DB name/user from deploy/.env if present, else fall back to defaults.
ENV_FILE="$SCRIPT_DIR/../deploy/.env"
POSTGRES_DB="smartcampus"
POSTGRES_USER="smartcampus"
if [ -f "$ENV_FILE" ]; then
  POSTGRES_DB="$(grep -E '^POSTGRES_DB=' "$ENV_FILE" | cut -d= -f2- || echo "$POSTGRES_DB")"
  POSTGRES_USER="$(grep -E '^POSTGRES_USER=' "$ENV_FILE" | cut -d= -f2- || echo "$POSTGRES_USER")"
fi

TIMESTAMP="$(date -u +%Y%m%dT%H%M%SZ)"
OUT_FILE="$BACKUP_DIR/smartcampus-$TIMESTAMP.sql.gz"

echo "==> Backing up '$POSTGRES_DB' from container '$CONTAINER_NAME' to $OUT_FILE"
docker exec "$CONTAINER_NAME" pg_dump -U "$POSTGRES_USER" "$POSTGRES_DB" | gzip > "$OUT_FILE"

SIZE="$(du -h "$OUT_FILE" | cut -f1)"
echo "==> Backup complete: $OUT_FILE ($SIZE)"

echo "==> Pruning backups older than $RETENTION_DAYS days"
find "$BACKUP_DIR" -name "smartcampus-*.sql.gz" -mtime "+$RETENTION_DAYS" -print -delete
