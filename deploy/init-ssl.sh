#!/bin/sh
# One-time bootstrap for a school's first Let's Encrypt certificate.
#
# Problem this solves: nginx's HTTPS server block needs a certificate file to exist
# before nginx will even start, but Let's Encrypt's HTTP-01 challenge needs nginx
# already running to serve it. Standard resolution: start nginx with a throwaway
# self-signed "dummy" cert just so it boots, request the real cert via the ACME
# HTTP challenge, then reload nginx onto the real cert.
#
# Run this once per school, after `deploy/.env` is filled in with the real
# DOMAIN_NAME and CERTBOT_EMAIL, and before this is the school's first-ever deploy.
# Safe to re-run — Certbot skips issuance if a valid cert already exists.
set -eu

cd "$(dirname "$0")"

# shellcheck disable=SC1091
. ./.env

if [ -z "${DOMAIN_NAME:-}" ] || [ "$DOMAIN_NAME" = "school.example.com" ]; then
  echo "Set a real DOMAIN_NAME in deploy/.env before running this." >&2
  exit 1
fi

echo "==> Creating a throwaway self-signed cert so nginx can start"
docker compose run --rm --entrypoint sh certbot -c "
  mkdir -p /etc/letsencrypt/live/$DOMAIN_NAME &&
  openssl req -x509 -nodes -newkey rsa:2048 -days 1 \
    -keyout /etc/letsencrypt/live/$DOMAIN_NAME/privkey.pem \
    -out /etc/letsencrypt/live/$DOMAIN_NAME/fullchain.pem \
    -subj '/CN=localhost'
"

echo "==> Starting nginx with the dummy cert"
docker compose up -d nginx

echo "==> Requesting the real certificate from Let's Encrypt"
docker compose run --rm certbot certonly \
  --webroot -w /var/www/certbot \
  --email "$CERTBOT_EMAIL" \
  -d "$DOMAIN_NAME" \
  --agree-tos --no-eff-email --force-renewal

echo "==> Reloading nginx onto the real certificate"
docker compose exec nginx nginx -s reload

echo "==> Done. Starting the certbot renewal sidecar."
docker compose up -d certbot

echo "SSL is live for https://$DOMAIN_NAME"
