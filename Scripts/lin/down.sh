#!/bin/bash
set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd -- "${SCRIPT_DIR}/../.." && pwd)"
ENV_PATH_FILE="${SCRIPT_DIR}/../path_to_env"

if [[ ! -f "${ENV_PATH_FILE}" ]]; then
  echo "File not found: ${ENV_PATH_FILE}" >&2
  exit 1
fi

ENV_PATH="$(sed -e 's/^[[:space:]]*//' -e 's/[[:space:]]*$//' "${ENV_PATH_FILE}" | head -n 1)"

if [[ -z "${ENV_PATH}" ]]; then
  echo "Scripts/path_to_env is empty. Put absolute path to dev.env there." >&2
  exit 1
fi

docker compose -f "${REPO_ROOT}/compose.yaml" --env-file "${ENV_PATH}" --project-name medistoreserver down
