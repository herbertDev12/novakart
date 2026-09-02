#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
source scripts/lib.sh

touched "^apps/web/" || { echo "→ No frontend changes, skipping build."; exit 0; }

echo "→ Frontend: lint"
pnpm --filter web run lint

echo "→ Frontend: build"
pnpm --filter web run build

echo "✓ Frontend build OK"
