#!/usr/bin/env bash
# Download every uploaded attachment on the Trello board into docs/trello/attachments/<card>/.
# Credentials come from .env (TRELLO_KEY, TRELLO_TOKEN) and are never printed.
set -euo pipefail

root="$(cd "$(dirname "$0")/.." && pwd)"
board="${1:-aawcUoCf}"
out="$root/docs/trello/attachments"

[ -f "$root/.env" ] || { echo "Missing $root/.env (see .env.example)" >&2; exit 1; }
set -a; . "$root/.env"; set +a
: "${TRELLO_KEY:?TRELLO_KEY not set in .env}" "${TRELLO_TOKEN:?TRELLO_TOKEN not set in .env}"

auth="Authorization: OAuth oauth_consumer_key=\"$TRELLO_KEY\", oauth_token=\"$TRELLO_TOKEN\""
api="https://api.trello.com/1"

cards="$(curl -fsS -H "$auth" "$api/boards/$board/cards/all?fields=name,closed&attachments=true&attachment_fields=name,url,isUpload")"

mkdir -p "$out"
printf '%s' "$cards" | jq -r '.[] | .name as $c | .attachments[] | select(.isUpload) | [$c, .id, .name, .url] | @tsv' |
while IFS=$'\t' read -r card id name url; do
  dir="$out/$(printf '%s' "$card" | tr -cs '[:alnum:]' '-' | sed 's/^-//;s/-$//' | cut -c1-60)"
  mkdir -p "$dir"
  dest="$dir/${id}-${name}"
  [ -s "$dest" ] && { echo "skip  $dest"; continue; }
  curl -fsSL -H "$auth" -o "$dest" "$url" && echo "saved $dest"
done
