#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
source scripts/lib.sh

touched "^apps/api/.*\.(cs|csproj|slnx|props)$" || {
  echo "→ No backend changes, skipping."; exit 0;
}

echo "→ Backend: format check"
# whitespace + style only, deliberately not the analyzer stage: `dotnet format
# analyzers` applies code fixes that can paper over real diagnostics (e.g.
# slapping [Obsolete] on a member to silence CS0618). Analyzer-level problems
# are caught by the build below, which treats warnings as errors.
dotnet format whitespace apps/api/NovaKart.slnx --verify-no-changes
dotnet format style apps/api/NovaKart.slnx --verify-no-changes

# Nullable + warnings-as-errors are compiler diagnostics, so they need a compile.
#
# The whole solution, not just the projects owning the changed files: `dotnet
# build <proj>` compiles that project's *dependencies*, never its *dependents*,
# so a breaking change in NovaKart.Domain used to compile clean here and only
# blow up at pre-push. Debug + incremental keeps this in the seconds range.
echo "→ Backend: compile (nullable + warnings as errors)"
dotnet build apps/api/NovaKart.slnx -c Debug --nologo -v minimal

echo "✓ Backend checks OK"
