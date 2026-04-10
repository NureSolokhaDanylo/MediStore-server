#!/bin/bash

set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
URL="${1:-http://localhost:14000/openapi/v1/openapi.json}"
OUT_DIR="$REPO_ROOT/OpenApiHistory"
OUT_FILE="$REPO_ROOT/openapi.json"
TMP_FILE="$(mktemp)"

mkdir -p "$OUT_DIR"

last_file="$(find "$OUT_DIR" -maxdepth 1 -type f -name 'openapi.v*.json' | sort | tail -n 1)"
next_version=1

if [[ -n "$last_file" ]]; then
  last_name="$(basename "$last_file")"
  last_version="${last_name#openapi.v}"
  last_version="${last_version%.json}"
  next_version=$((10#$last_version + 1))
fi

VERSION_FILE="$OUT_DIR/openapi.v$(printf '%04d' "$next_version").json"

echo "Downloading OpenAPI specification from $URL..."

if ! curl --fail --show-error --silent \
  -H 'Accept: application/json' \
  -H 'Cache-Control: no-cache' \
  -H 'Pragma: no-cache' \
  "$URL" > "$TMP_FILE"; then
  rm -f "$TMP_FILE"
  echo "Error: Failed to download OpenAPI spec. Is the server running on port 14000?"
  exit 1
fi

cp "$TMP_FILE" "$VERSION_FILE"
mv "$TMP_FILE" "$OUT_FILE"

echo "✓ Saved to: $OUT_FILE"
echo "✓ Archived snapshot: $VERSION_FILE"
echo "File size: $(wc -c < "$OUT_FILE") bytes"
echo "Lines: $(wc -l < "$OUT_FILE")"
