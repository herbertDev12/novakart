#!/usr/bin/env bash

DEFAULT_BRANCH="${DEFAULT_BRANCH:-main}"

# Files relevant to the current hook stage. HOOK_STAGE is set by the hook.
#
# These are working-tree paths, and the checks that consume them (dotnet format,
# dotnet build, tsc) read the working tree — not the index. So `git add -p` and
# unrelated dirty files are in scope. That is deliberate: the usual fix is to
# wrap the hook in `git stash --keep-index`, but an interrupted hook (Ctrl-C, a
# crash, a build that hangs) can then strand your unstaged work in a stash you
# have to know to go looking for. Seeing the whole tree is the safer failure.
changed_files() {
  case "${HOOK_STAGE:-commit}" in
    commit)
      git diff --cached --name-only --diff-filter=ACMR
      ;;
    push)
      # PUSH_RANGES is set by .husky/pre-push from the refs git hands it on
      # stdin, so we check the commits actually being pushed. Space-separated;
      # a range is `<oid>...<oid>` and never contains whitespace.
      if [ -n "${PUSH_RANGES:-}" ]; then
        for range in $PUSH_RANGES; do
          git diff --name-only --diff-filter=ACMR "$range"
        done | sort -u
      elif git rev-parse --abbrev-ref --symbolic-full-name '@{u}' >/dev/null 2>&1; then
        git diff --name-only --diff-filter=ACMR '@{u}...HEAD'
      elif git rev-parse --verify "origin/${DEFAULT_BRANCH}" >/dev/null 2>&1; then
        # New branch with no upstream yet: diff against the default branch.
        git diff --name-only --diff-filter=ACMR \
          "$(git merge-base "origin/${DEFAULT_BRANCH}" HEAD)...HEAD"
      else
        # No remote reference at all — check everything rather than nothing.
        git ls-files
      fi
      ;;
    *)
      # Unknown stage. Check everything rather than silently skipping every
      # check, which is what an unset-but-non-empty HOOK_STAGE used to do.
      git ls-files
      ;;
  esac
}

# touched <regex>  →  0 if any changed file matches
touched() {
  changed_files | grep -qE "$1"
}
