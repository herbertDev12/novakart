#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
source scripts/lib.sh

touched "^apps/web/" || { echo "→ No frontend changes, skipping."; exit 0; }

echo "→ Frontend: type check"
pnpm --filter web run check-types

echo "✓ Frontend checks OK"
