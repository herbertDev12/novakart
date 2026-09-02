#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
source scripts/lib.sh

touched "^apps/api/.*\.(cs|csproj|slnx|props)$" || {
  echo "→ No backend changes, skipping build."; exit 0;
}

echo "→ Backend: build (warnings as errors)"
dotnet build apps/api/NovaKart.slnx -c Release --nologo

# The integration tests need Docker (testcontainers spins up postgres). Warn
# loudly and carry on rather than walling you off from pushing when the daemon
# happens to be down — this repo has no CI, so a hard block here has no backstop.
if docker info >/dev/null 2>&1; then
  echo "→ Backend: integration tests"
  # --no-build reuses the Release output above. </dev/null keeps the test host
  # away from the ref list git feeds pre-push on stdin.
  dotnet test apps/api/NovaKart.slnx -c Release --no-build --nologo </dev/null
else
  echo "⚠  Docker is not available — SKIPPING integration tests."
  echo "⚠  Start Docker and re-push, or run: dotnet test apps/api/NovaKart.slnx"
fi

echo "✓ Backend build OK"
