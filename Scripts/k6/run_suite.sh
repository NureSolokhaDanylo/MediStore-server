#!/usr/bin/env bash
set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
RESULTS_ROOT="${RESULTS_ROOT:-$SCRIPT_DIR/results}"
CONFIG_LABEL="${CONFIG_LABEL:-}"

if [[ -z "$CONFIG_LABEL" ]]; then
  echo "CONFIG_LABEL is required (example: C1, C2, C3, C4)"
  exit 1
fi

BASE_URL="${BASE_URL:-https://api.medistore.app}"

RUN_ID="$(date +%Y%m%d_%H%M%S)"
OUT_DIR="$RESULTS_ROOT/$CONFIG_LABEL/$RUN_ID"
mkdir -p "$OUT_DIR"

READ_RATE="${READ_RATE:-80}"
READ_DURATION="${READ_DURATION:-2m}"
WRITE_RATE="${WRITE_RATE:-10}"
WRITE_DURATION="${WRITE_DURATION:-2m}"
READ_LOW_RATE="${READ_LOW_RATE:-20}"
READ_LOW_DURATION="${READ_LOW_DURATION:-2m}"
WRITE_LOW_RATE="${WRITE_LOW_RATE:-3}"
WRITE_LOW_DURATION="${WRITE_LOW_DURATION:-2m}"

echo "== Running k6 suite =="
echo "Config      : $CONFIG_LABEL"
echo "Base URL    : $BASE_URL"
echo "Output dir  : $OUT_DIR"
echo "Read high   : rate=$READ_RATE rps, duration=$READ_DURATION"
echo "Write high  : rate=$WRITE_RATE rps, duration=$WRITE_DURATION"
echo "Read low    : rate=$READ_LOW_RATE rps, duration=$READ_LOW_DURATION"
echo "Write low   : rate=$WRITE_LOW_RATE rps, duration=$WRITE_LOW_DURATION"

echo "\n--- [1/4] READ high ---"
echo "Logging to  : $OUT_DIR/read_mix.log"
if ! k6 run "$SCRIPT_DIR/read_mix.js" \
  -e READ_RATE="$READ_RATE" \
  -e READ_DURATION="$READ_DURATION" \
  --summary-export "$OUT_DIR/read_mix.summary.json" \
  >"$OUT_DIR/read_mix.log" 2>&1; then
  echo "READ mix finished with non-zero exit code; see $OUT_DIR/read_mix.log"
fi

echo "\n--- [2/4] WRITE high ---"
echo "Logging to  : $OUT_DIR/write_readings.log"
if ! k6 run "$SCRIPT_DIR/write_readings.js" \
  -e WRITE_RATE="$WRITE_RATE" \
  -e WRITE_DURATION="$WRITE_DURATION" \
  --summary-export "$OUT_DIR/write_readings.summary.json" \
  >"$OUT_DIR/write_readings.log" 2>&1; then
  echo "WRITE readings finished with non-zero exit code; see $OUT_DIR/write_readings.log"
fi

echo "\n--- [3/4] READ low ---"
echo "Logging to  : $OUT_DIR/read_mix_low.log"
if ! k6 run "$SCRIPT_DIR/read_mix.js" \
  -e READ_RATE="$READ_LOW_RATE" \
  -e READ_DURATION="$READ_LOW_DURATION" \
  --summary-export "$OUT_DIR/read_mix_low.summary.json" \
  >"$OUT_DIR/read_mix_low.log" 2>&1; then
  echo "READ low finished with non-zero exit code; see $OUT_DIR/read_mix_low.log"
fi

echo "\n--- [4/4] WRITE low ---"
echo "Logging to  : $OUT_DIR/write_readings_low.log"
if ! k6 run "$SCRIPT_DIR/write_readings.js" \
  -e WRITE_RATE="$WRITE_LOW_RATE" \
  -e WRITE_DURATION="$WRITE_LOW_DURATION" \
  --summary-export "$OUT_DIR/write_readings_low.summary.json" \
  >"$OUT_DIR/write_readings_low.log" 2>&1; then
  echo "WRITE low finished with non-zero exit code; see $OUT_DIR/write_readings_low.log"
fi

echo "\nSuite complete: $OUT_DIR"
echo "Build markdown table after all configs:"
echo "python3 $SCRIPT_DIR/summary_table.py $RESULTS_ROOT --baseline C1"
