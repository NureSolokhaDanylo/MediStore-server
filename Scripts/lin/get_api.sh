#!/bin/bash

set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
URL="${1:-http://localhost:14000/openapi/v1/openapi.json}"
OUT_FILE="$REPO_ROOT/openapi.json"

echo "Downloading OpenAPI specification from $URL..."

if ! curl --fail --show-error --silent \
  -H 'Accept: application/json' \
  -H 'Cache-Control: no-cache' \
  -H 'Pragma: no-cache' \
  "$URL" > "$OUT_FILE"; then
  echo "Error: Failed to download OpenAPI spec. Is the server running on port 14000?"
  exit 1
fi

echo "✓ Saved to: $OUT_FILE"
echo "File size: $(wc -c < "$OUT_FILE") bytes"
echo "Lines: $(wc -l < "$OUT_FILE")"
