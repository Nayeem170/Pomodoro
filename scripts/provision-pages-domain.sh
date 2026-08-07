#!/usr/bin/env bash
set -euo pipefail

PROJECT_NAME="${PROJECT_NAME:?PROJECT_NAME is required}"
CUSTOM_DOMAIN="${CUSTOM_DOMAIN:?CUSTOM_DOMAIN is required}"
ZONE_NAME="${ZONE_NAME:?ZONE_NAME is required}"
CF_API_TOKEN="${CF_API_TOKEN:?CF_API_TOKEN is required}"
CF_ACCOUNT_ID="${CF_ACCOUNT_ID:?CF_ACCOUNT_ID is required}"

API="https://api.cloudflare.com/client/v4"

log() { echo "[provision] $*"; }

cf_get() { curl -sf -X GET "$1" -H "Authorization: Bearer $CF_API_TOKEN"; }
cf_post() { curl -sf -X POST "$1" -H "Authorization: Bearer $CF_API_TOKEN" -H "Content-Type: application/json" -d "$2"; }

ZONE_ID=$(cf_get "$API/zones?name=$ZONE_NAME" | jq -r '.result[0].id // empty')
if [ -z "$ZONE_ID" ]; then
  log "ERROR: zone '$ZONE_NAME' not found"
  exit 1
fi
log "zone $ZONE_NAME -> $ZONE_ID"

DOMAINS_URL="$API/accounts/$CF_ACCOUNT_ID/pages/projects/$PROJECT_NAME/domains"
DOMAIN_EXISTS=$(cf_get "$DOMAINS_URL" | jq -r --arg d "$CUSTOM_DOMAIN" '.result[] | select(.name==$d) | .name')
if [ -z "$DOMAIN_EXISTS" ]; then
  log "custom domain $CUSTOM_DOMAIN not on project, creating..."
  cf_post "$DOMAINS_URL" "{\"name\":\"$CUSTOM_DOMAIN\"}" >/dev/null
  log "custom domain created"
else
  log "custom domain already attached"
fi

CNAME_TARGET="$PROJECT_NAME.pages.dev"
DNS_URL="$API/zones/$ZONE_ID/dns_records?name=$CUSTOM_DOMAIN&type=CNAME"
CNAME_EXISTS=$(cf_get "$DNS_URL" | jq -r '.result[0].name // empty')
if [ -z "$CNAME_EXISTS" ]; then
  log "CNAME $CUSTOM_DOMAIN -> $CNAME_TARGET not found, creating..."
  cf_post "$API/zones/$ZONE_ID/dns_records" \
    "{\"type\":\"CNAME\",\"name\":\"$CUSTOM_DOMAIN\",\"content\":\"$CNAME_TARGET\",\"proxied\":true}" >/dev/null
  log "CNAME created"
else
  log "CNAME already exists"
fi

log "domain provisioning complete: https://$CUSTOM_DOMAIN"
