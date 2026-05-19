#!/usr/bin/env bash
set -euo pipefail

BASE_URL="http://127.0.0.1:40913"
INPUT="${1:-parsed.csv}"
OUTPUT_DIR="${2:-texts}"

mkdir -p "$OUTPUT_DIR"

tail -n +2 "$INPUT" | while IFS=, read -r path_id class name name_href collection collection_href; do
    path="${name_href/\/Assets\/View/\/Assets\/Text}"
    out="${OUTPUT_DIR}/${path_id}-${name}.txt"
    echo "Fetching ${path_id}-${name}..."
    curl -s -f -o "$out" "${BASE_URL}${path}"
done
