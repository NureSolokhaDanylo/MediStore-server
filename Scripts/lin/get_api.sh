#!/bin/bash

set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
URL="${1:-http://localhost:14000/swagger/v1/swagger.json}"

curl --fail --show-error --silent \
  -H 'Accept: application/json' \
  -H 'Cache-Control: no-cache' \
  -H 'Pragma: no-cache' \
  "$URL" > swagger.notes

echo "Saved: $OUT_FILE"
wc -l "$OUT_FILE"
