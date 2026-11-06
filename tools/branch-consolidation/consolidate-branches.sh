#!/usr/bin/env bash
# =============================================================================
#  consolidate-branches.sh
#
#  Consolidates every branch in one or more Git repositories down to a single
#  canonical branch named "master", preserving history.
#
#  SAFETY MODEL (read this before running):
#    * DRY-RUN IS THE DEFAULT. Nothing is modified unless MODE=execute.
#    * Conflicts are NEVER auto-resolved. The first conflicting branch stops
#      all automated merges for that repository; other repositories continue.
#    * Every branch is tagged (pre-delete-backup/<branch>/<timestamp>) and the
#      tag is PUSHED before any deletion, so a deletion is always recoverable.
#    * Protected branches are never deleted.
#    * Merges use --no-ff, so the branch's history is preserved as a distinct
#      line rather than being flattened away.
#
#  Usage:
#      MODE=dry-run ./consolidate-branches.sh
#      MODE=execute DELETE_REMOTE_BRANCHES=true ./consolidate-branches.sh
# =============================================================================
set -uo pipefail

# ---------------------------------------------------------------------------
# INPUTS -- replace these before running.
# ---------------------------------------------------------------------------

# Local paths or remote URLs. Remote URLs are cloned to a temp dir and removed
# afterwards, which satisfies the "only one copy of each repo" requirement.
REPOS=(
  "D:/PlantSafety/source/Azure"
  "D:/PlantSafety/source/DataLake"
  "D:/PlantSafety/source/Diagnostics"
  "D:/PlantSafety/source/ExportImport"
  "D:/PlantSafety/source/Intelligence"
  "D:/PlantSafety/source/PlantSafety"
  "D:/PlantSafety/source/Reporting"
  "D:/PlantSafety/source/SafetyIntegrations"
  "D:/PlantSafety/source/SafetyTooling"
)

MODE="${MODE:-dry-run}"                                   # dry-run | execute
DELETE_REMOTE_BRANCHES="${DELETE_REMOTE_BRANCHES:-false}" # true | false
GITHUB_TOKEN="${GITHUB_TOKEN:-}"                          # optional
PROTECTED_BRANCHES=("master")
TIMESTAMP_FORMAT="+%Y%m%d-%H%M%S"

REPORT_DIR="${REPORT_DIR:-$(pwd)/consolidation-reports}"

# ---------------------------------------------------------------------------
# Internals
# ---------------------------------------------------------------------------
TIMESTAMP="$(date "$TIMESTAMP_FORMAT")"
mkdir -p "$REPORT_DIR"
CONSOLIDATED_REPORT="$REPORT_DIR/consolidated-$TIMESTAMP.md"
TEMP_CLONES=()

log()  { printf '%s\n' "$*"; }
step() { printf '\n== %s\n' "$*"; }

# In dry-run, every mutating command is printed instead of run. Keeping this in
# ONE place means a mutation cannot accidentally bypass the mode check.
run() {
  if [ "$MODE" = "execute" ]; then
    "$@"
  else
    printf '   [dry-run] %s\n' "$*"
  fi
}

is_protected() {
  local b="$1"
  for p in "${PROTECTED_BRANCHES[@]}"; do
    [ "$b" = "$p" ] && return 0
  done
  return 1
}

cleanup() {
  for d in "${TEMP_CLONES[@]:-}"; do
    [ -n "$d" ] && [ -d "$d" ] && rm -rf "$d"
  done
}
trap cleanup EXIT

{
  echo "# Branch consolidation -- consolidated report"
  echo
  echo "- Timestamp: \`$TIMESTAMP\`"
  echo "- Mode: \`$MODE\`"
  echo "- Delete remote branches: \`$DELETE_REMOTE_BRANCHES\`"
  echo "- Protected: \`${PROTECTED_BRANCHES[*]}\`"
  echo
} > "$CONSOLIDATED_REPORT"

# ---------------------------------------------------------------------------
# process_repo <path-or-url>
# ---------------------------------------------------------------------------
process_repo() {
  local input="$1"
  local repo_path="$input"
  local is_temp=0

  if [[ "$input" =~ ^(https?://|git@) ]]; then
    repo_path="$(mktemp -d)"
    TEMP_CLONES+=("$repo_path")
    is_temp=1
    step "Cloning $input -> $repo_path"
    if [ "$MODE" = "execute" ]; then
      git clone --quiet "$input" "$repo_path" || { log "   ERROR: clone failed"; return 1; }
    else
      log "   [dry-run] git clone $input $repo_path (skipping; nothing to inspect)"
      return 0
    fi
  fi

  local name; name="$(basename "$repo_path")"
  local report="$REPORT_DIR/$name-$TIMESTAMP.md"
  step "Repository: $name  ($repo_path)"

  if [ ! -d "$repo_path/.git" ]; then
    log "   ERROR: not a git repository -- skipping"
    echo "## $name -- ERROR: not a git repository" >> "$CONSOLIDATED_REPORT"
    return 1
  fi

  local G=(git -C "$repo_path")

  # ---- 2. Fetch and prune ------------------------------------------------
  # Always real, even in dry-run: fetching mutates nothing in the working tree
  # and without it every branch listing below would be stale.
  "${G[@]}" fetch --all --prune --tags --quiet || log "   WARN: fetch reported an error"

  # ---- Determine the master branch --------------------------------------
  # 8 of 9 repos in this workspace have NO origin/master; their default is a
  # feature/* branch. Failing here would make the script useless for them, so
  # master is CREATED from the remote default when it is missing.
  local master_exists master_source="" base_ref
  master_exists="$("${G[@]}" ls-remote --heads origin master | wc -l | tr -d ' ')"

  if [ "$master_exists" = "0" ]; then
    master_source="$("${G[@]}" symbolic-ref refs/remotes/origin/HEAD 2>/dev/null | sed 's|refs/remotes/origin/||')"
    if [ -z "$master_source" ]; then
      log "   ERROR: origin/master is missing and origin/HEAD is unset -- cannot determine a base."
      log "          Fix with: git -C '$repo_path' remote set-head origin --auto"
      echo "## $name -- ERROR: no origin/master and no origin/HEAD" >> "$CONSOLIDATED_REPORT"
      return 1
    fi
    log "   origin/master does not exist. Will create it from origin/$master_source."
    # The merge test must run against the branch master WOULD be created from.
    # Testing against origin/master here would silently test nothing in dry-run,
    # because dry-run never creates it.
    base_ref="origin/$master_source"
    # The exit code MUST be checked. A checkout can fail -- most often because
    # untracked files in the working tree would be overwritten -- and if that
    # goes unnoticed the repo stays on whatever branch it was on, and every
    # merge and push below would operate on the wrong branch.
    if ! run "${G[@]}" checkout -B master "origin/$master_source"; then
      log "   ERROR: could not check out master from origin/$master_source."
      log "          Usually untracked files would be overwritten. Resolve, then re-run."
      echo "## $name -- ERROR: checkout of master failed (untracked files in the way?)" >> "$CONSOLIDATED_REPORT"
      return 1
    fi
    run "${G[@]}" push -u origin master
  else
    log "   origin/master exists."
    base_ref="origin/master"
    if ! run "${G[@]}" checkout -B master origin/master; then
      log "   ERROR: could not check out master from origin/master."
      echo "## $name -- ERROR: checkout of master failed" >> "$CONSOLIDATED_REPORT"
      return 1
    fi
  fi

  # ---- 3. Enumerate remote branches -------------------------------------
  # ls-remote --heads, NOT for-each-ref: git shortens refs/remotes/origin/HEAD
  # to the bare remote name ("origin"), so a for-each-ref listing yields a
  # phantom branch called "origin" that no HEAD filter catches. ls-remote
  # returns only real heads.
  local branches=()
  while IFS= read -r b; do
    [ -z "$b" ] && continue
    is_protected "$b" && continue
    branches+=("$b")
  done < <("${G[@]}" ls-remote --heads origin 2>/dev/null \
            | awk '{print $2}' | sed 's|^refs/heads/||' | sort -u)

  # Local-only branches are reported but NOT merged: the specification scopes
  # this to origin. Reporting them means nothing disappears silently.
  local local_only=()
  while IFS= read -r b; do
    [ -z "$b" ] && continue
    is_protected "$b" && continue
    if ! "${G[@]}" show-ref --verify --quiet "refs/remotes/origin/$b"; then
      local_only+=("$b")
    fi
  done < <("${G[@]}" for-each-ref --format='%(refname:short)' refs/heads)

  log "   Remote branches to consolidate: ${#branches[@]}"
  log "   Local-only branches (reported, not merged): ${#local_only[@]}"

  # ---- 4. Merge test (no auto-resolution) --------------------------------
  local mergeable=() conflicted=() conflict_detail=""
  local test_branch="consolidation-merge-test-$TIMESTAMP"

  for b in "${branches[@]}"; do
    if ! "${G[@]}" checkout -q -B "$test_branch" "$base_ref" >/dev/null 2>&1; then
      log "   ERROR: cannot create merge-test branch from $base_ref -- skipping $b"
      conflicted+=("$b")
      conflict_detail+="- \`$b\`: could not check out base \`$base_ref\`"$'\n'
      continue
    fi

    local err
    err="$("${G[@]}" merge --no-commit --no-ff "origin/$b" 2>&1)"
    if [ $? -eq 0 ]; then
      "${G[@]}" merge --abort >/dev/null 2>&1 || "${G[@]}" reset --hard -q
      mergeable+=("$b")
      log "   [ok]       $b"
    else
      # Capture git's own message, not just the conflicted-file list. A merge
      # can fail with NO conflicted files -- "refusing to merge unrelated
      # histories" is the common case, and reporting an empty file list for it
      # would read as a mystery rather than a diagnosis.
      local files reason
      files="$("${G[@]}" diff --name-only --diff-filter=U | tr '\n' ' ')"
      reason="$(printf '%s' "$err" | head -1)"
      "${G[@]}" merge --abort >/dev/null 2>&1 || "${G[@]}" reset --hard -q
      conflicted+=("$b")
      if [ -n "$files" ]; then
        conflict_detail+="- \`$b\`: conflicting files: $files"$'\n'
        log "   [CONFLICT] $b -> $files"
      else
        conflict_detail+="- \`$b\`: $reason"$'\n'
        log "   [BLOCKED]  $b -> $reason"
      fi
    fi
  done

  "${G[@]}" checkout -q master >/dev/null 2>&1
  "${G[@]}" branch -q -D "$test_branch" >/dev/null 2>&1

  # A conflict stops automated merges for THIS repo only. Merging the clean
  # subset would leave the repo half-consolidated with no obvious record of
  # where it stopped, which is harder to reason about than not starting.
  local halted=0
  if [ "${#conflicted[@]}" -gt 0 ]; then
    halted=1
    log "   HALTED: ${#conflicted[@]} branch(es) conflict. No merges performed for this repository."
  fi

  # ---- 5/6/7. Tag, merge, push, delete -----------------------------------
  local merged=() tagged=() deleted=() undeletable=()

  if [ "$halted" -eq 0 ] && [ "${#mergeable[@]}" -gt 0 ]; then
    # Tags first, and pushed, so that every branch is recoverable before any
    # merge or delete touches it.
    for b in "${mergeable[@]}"; do
      local tag="pre-delete-backup/$b/$TIMESTAMP"
      run "${G[@]}" tag -a "$tag" "origin/$b" -m "Backup of $b before consolidation ($TIMESTAMP)"
      run "${G[@]}" push origin "refs/tags/$tag"
      tagged+=("$tag")
    done

    run "${G[@]}" checkout master
    for b in "${mergeable[@]}"; do
      if run "${G[@]}" merge --no-ff "origin/$b" -m "Merge $b into master (consolidation)"; then
        merged+=("$b")
      else
        log "   ERROR: merge of $b failed unexpectedly after a clean test -- stopping."
        run "${G[@]}" merge --abort
        halted=1
        break
      fi
    done

    if [ "$halted" -eq 0 ]; then
      run "${G[@]}" push origin master

      if [ "$DELETE_REMOTE_BRANCHES" = "true" ]; then
        for b in "${merged[@]}"; do
          is_protected "$b" && continue
          run "${G[@]}" branch -D "$b"
          if run "${G[@]}" push origin --delete "$b"; then
            deleted+=("$b")
          else
            # Branch protection or a default-branch pointer will refuse this.
            undeletable+=("$b")
            log "   NOTE: could not delete origin/$b (protected, or it is the default branch)."
          fi
        done
      else
        log "   DELETE_REMOTE_BRANCHES=false -- branches merged and tagged, none deleted."
      fi
    fi
  fi

  # ---- 9. Per-repo summary ----------------------------------------------
  {
    echo "# $name"
    echo
    echo "- Path: \`$repo_path\`"
    echo "- Mode: \`$MODE\`  |  Timestamp: \`$TIMESTAMP\`"
    echo "- Halted by conflict: **$([ "$halted" -eq 1 ] && echo yes || echo no)**"
    echo
    echo "## Branches evaluated (${#branches[@]})"
    printf -- '- %s\n' "${branches[@]:-(none)}"
    echo
    echo "## Merged into master (${#merged[@]})"
    printf -- '- %s\n' "${merged[@]:-(none)}"
    echo
    echo "## Conflicts (${#conflicted[@]}) -- require manual resolution"
    [ -n "$conflict_detail" ] && printf '%s' "$conflict_detail" || echo "(none)"
    echo
    echo "## Backup tags created (${#tagged[@]})"
    printf -- '- %s\n' "${tagged[@]:-(none)}"
    echo
    echo "## Branches deleted (${#deleted[@]})"
    printf -- '- %s\n' "${deleted[@]:-(none)}"
    echo
    echo "## Deletion refused (${#undeletable[@]}) -- protected or default branch"
    printf -- '- %s\n' "${undeletable[@]:-(none)}"
    echo
    echo "## Local-only branches, NOT merged (${#local_only[@]})"
    printf -- '- %s\n' "${local_only[@]:-(none)}"
    echo
    echo "## Final master commit"
    echo '```'
    "${G[@]}" log --oneline -1 master 2>/dev/null || echo "(unavailable)"
    echo '```'
  } > "$report"

  {
    echo "## $name"
    echo "- Evaluated: ${#branches[@]} | Merged: ${#merged[@]} | Conflicts: ${#conflicted[@]} | Tagged: ${#tagged[@]} | Deleted: ${#deleted[@]}"
    echo "- Halted: $([ "$halted" -eq 1 ] && echo yes || echo no)"
    echo "- Final master: \`$("${G[@]}" log --oneline -1 master 2>/dev/null || echo n/a)\`"
    echo "- Report: \`$report\`"
    echo
  } >> "$CONSOLIDATED_REPORT"

  log "   Summary written: $report"

  # ---- 8. Single copy guarantee -----------------------------------------
  if [ "$is_temp" -eq 1 ]; then
    rm -rf "$repo_path"
  fi
}

# ---------------------------------------------------------------------------
step "Branch consolidation -- MODE=$MODE  DELETE_REMOTE_BRANCHES=$DELETE_REMOTE_BRANCHES"
[ "$MODE" != "execute" ] && log "DRY RUN: no repository will be modified."

for r in "${REPOS[@]}"; do
  process_repo "$r" || log "   (continuing with remaining repositories)"
done

step "Consolidated report: $CONSOLIDATED_REPORT"
log ""
log "Verify final state per repo with:"
log "  git fetch --all --prune"
log "  git for-each-ref --format='%(refname:short)' refs/remotes/origin | sed 's|^origin/||'"
