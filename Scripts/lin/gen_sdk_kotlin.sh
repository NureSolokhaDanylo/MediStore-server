#!/bin/bash

set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
SPEC_FILE="${1:-$REPO_ROOT/openapi.json}"
OUT_DIR="$REPO_ROOT/GeneratedClients/kotlin"

echo "Generating Kotlin SDK from $SPEC_FILE..."
rm -rf "$OUT_DIR"
openapi-generator-cli generate \
  -i "$SPEC_FILE" \
  -g kotlin \
  -p library=jvm-retrofit2 \
  -o "$OUT_DIR"

echo "✓ Kotlin SDK generated in: $OUT_DIR"
