# Branch consolidation — one canonical `master` per repository

Merges every branch in a repository into a single canonical `master`, preserving
history, then deletes the rest — with a backup tag pushed for each branch first.

**Dry-run is the default everywhere.** Nothing is modified until you ask for it.

---

## Read this first: the state of these repositories

A dry run against the nine workspace repositories found three things that change
how you should approach this.

**1. Eight of nine repositories have no `master` at all.**

| Repository | `origin/master` | Default branch |
|---|---|---|
| Reporting | yes | `feature/update-reporting` |
| PlantSafety | no | `claude/module-audit-buildout` |
| Azure, DataLake, Diagnostics, ExportImport, Intelligence, SafetyIntegrations, SafetyTooling | no | `feature/update-*` |

"Merge every branch into master" has no master to merge into. The scripts
therefore **create** `master` from each repository's current default branch and
push it, rather than failing. That is a new branch appearing on your remotes —
expected, but worth knowing before you run in execute mode.

**2. Two repositories will not consolidate cleanly.**

- **PlantSafety** — `feature/update-plantsafety` and `claude/module-audit-buildout`
  conflict across 21 files, including `PlantSafety.UI.csproj`,
  `PlantSafety.IntegrationTests/Setup/TestDatabaseFactory.cs`,
  `InitTestHost.cs` and `.gitignore`. These are two genuinely divergent lines of
  development, not a formatting clash. Resolve by hand.
- **Reporting** — `feature/setup-repositories` is blocked because untracked files
  in the working tree would be overwritten. Clean or stash the working tree first;
  this one is not a history conflict.

**2b. There are NO untracked-file collisions blocking checkout.**

An earlier draft of this file claimed four repositories (SafetyTooling 28,
PlantSafety 10, DataLake 1, Diagnostics 1) had untracked files colliding with
their target branch. **That was wrong.** It was measured by directory prefix, and
git only refuses a checkout on *file*-level collisions.

Re-measured at file level with `git status -uall`, the count is **zero in every
repository**:

| Repository | Untracked files | File-level collisions |
|---|---|---|
| PlantSafety | 18,299 | 0 |
| SafetyTooling | 11,342 | 0 |
| Diagnostics | 6,676 | 0 |
| Reporting | 7,768 | 0 |
| Azure | 4,208 | 0 |
| DataLake | 812 | 0 |

The flagged directories (`DataLake.Api/`, `PlantSafety.SecurityTests/`,
`SafetyReporting.Pdf/`, …) contain only build output; the source files their target
branches track are not present in the working tree at all. Checkout proceeds.

**3. Most branches are local-only.** Each remote carries one to three branches,
while PlantSafety alone has 20 local `claude/*` branches. The scripts consolidate
**remote** branches, per the specification, and *report* local-only branches
without touching them. Push any local branch you want included first, or it will
be left behind.

**4. Every repository is checked out on a junk `master`, and the real source is
currently untracked.**

Each repository has a local `master` that is an unrelated "Initial commit" — a raw
folder snapshot including `.vs/`, `bin/` and `obj/`. DataLake's tracks 1,004 such
files against 809 on the real work branch.

| Repository | `master` HEAD | What it is |
|---|---|---|
| DataLake | `5633442 Initial commit` | folder snapshot, 1,004 files |
| Azure | `477e57b Initial commit` | folder snapshot |
| PlantSafety | `fefb1e57 Initial commit for PlantSafety` | folder snapshot |
| Reporting | `a03b622 initial commit` | folder snapshot — **was also `origin/master`** |

Because the working trees sit on these, the actual source is untracked (18,299
files in PlantSafety, 11,342 in SafetyTooling).

> **Do not run `git clean -fd` in any of these repositories.** It would delete the
> working source. Wait until consolidation has completed and `master` holds the
> real history.

For the eight repositories with no `origin/master`, the scripts reset `master` to
the real work branch and push it, which discards these snapshot pointers — the
outcome you want.

**Reporting needed manual handling**, because its snapshot commit *was* published
as `origin/master`, so the scripts would have merged real work into a folder dump.
It has been tagged and is recoverable:

```
pre-consolidation-backup/origin-master/20260811-174340 -> a03b6227
```

Delete the branch so the script recreates it from the real work branch:

```bash
git -C <repo>/Reporting push origin --delete master
```

---

## Manual steps

### Command line

```bash
# 1. Refresh and see exactly what exists.
git fetch --all --prune --tags
git ls-remote --heads origin | awk '{print $2}' | sed 's|^refs/heads/||'
```

```bash
# 2. If master does not exist, create it from the current default branch.
git checkout -B master origin/$(git symbolic-ref refs/remotes/origin/HEAD | sed 's|refs/remotes/origin/||')
git push -u origin master
```

```bash
# 3. Test a merge WITHOUT committing. Repeat per branch.
git checkout -B merge-test origin/master
git merge --no-commit --no-ff origin/<branch>
```

If it conflicts, stop and resolve by hand — never let a tool auto-resolve:

```bash
git merge --abort
```

```bash
# 4. Back the branch up BEFORE any deletion, and push the tag.
git tag -a "pre-delete-backup/<branch>/$(date +%Y%m%d-%H%M%S)" origin/<branch> -m "Backup before consolidation"
git push origin --tags
```

```bash
# 5. Merge for real, preserving the branch's history as its own line.
git checkout master
git merge --no-ff origin/<branch> -m "Merge <branch> into master (consolidation)"
git push origin master
```

```bash
# 6. Delete, local then remote. Only after the tag is pushed.
git branch -D <branch>
git push origin --delete <branch>
```

```bash
# 7. Verify.
git fetch --all --prune
git for-each-ref --format='%(refname:short)' refs/remotes/origin
```

**Retarget the default branch before deleting it.** GitHub refuses to delete a
repository's default branch. Set the default to `master` under
*Settings → Branches* first, or the deletion silently fails and you are left with
two branches.

### GitHub Desktop

1. **Repository → Fetch origin.**
2. **Branch → New branch**, name it `master`, base it on the current branch. Publish it.
3. For each other branch: **Branch → Merge into current branch**, pick the branch, merge.
   Desktop shows conflicts before committing — resolve them in your editor, or **Abort**.
4. **Push origin.**
5. Tag from the command line — Desktop cannot create annotated tags:
   `git tag -a pre-delete-backup/<branch>/<timestamp> origin/<branch> -m "..." && git push origin --tags`
6. **Branch → Delete**, and tick *also delete on remote*.
7. On github.com, set the default branch to `master` under *Settings → Branches*.

---

## Scripts

| File | Purpose |
|---|---|
| `consolidate-branches.sh` | Bash. `MODE=dry-run` (default) or `MODE=execute`. |
| `Consolidate-Branches.ps1` | PowerShell, identical behaviour. |
| `consolidate-branches.yml` | Optional GitHub Actions workflow; copy to `.github/workflows/`. |

```bash
MODE=dry-run ./consolidate-branches.sh
```

```bash
MODE=execute DELETE_REMOTE_BRANCHES=true ./consolidate-branches.sh
```

```powershell
.\Consolidate-Branches.ps1 -Mode execute -DeleteRemoteBranches
```

Both write per-repository reports and a consolidated report to
`./consolidation-reports/`.

### What the scripts guarantee

- Dry-run modifies nothing. Every mutating command routes through one wrapper, so
  a mutation cannot bypass the mode check.
- Conflicts are never auto-resolved. The first conflicting branch halts all merges
  **for that repository**; the others continue.
- A merge that fails with no conflicted files (typically *refusing to merge
  unrelated histories*) is reported with git's own message rather than an empty list.
- Backup tags are created and **pushed before** any merge or delete.
- Protected branches are never deleted.
- Remote URLs are cloned to a temp directory and removed afterwards.
- A branch that cannot be deleted (protection rules, or it is the default) is
  reported, not silently skipped.

### The GitHub Actions workflow

Manual dispatch only, dry-run by default. Execute mode routes through a GitHub
Environment named `branch-consolidation` — configure it with required reviewers so
a human approves before anything is written. Without that environment, execute
mode waits and never runs. Any conflict fails the job with nothing modified.

---

## Preconditions

- [ ] Every repository is committed and clean of *tracked* changes
      (`git status --porcelain | grep -v '^??'` empty). All nine currently pass this.
      Untracked build output is fine — it causes no checkout collisions (see 2b) —
      but it does block Reporting's `feature/setup-repositories` **merge**, which is
      a separate failure from checkout. Clean or stash before that one.
- [ ] Reporting's published snapshot `origin/master` has been tagged and deleted
      (see 4), or consolidation will merge real work into a folder dump.
- [ ] Any local branch you want kept has been pushed. Local-only branches are
      reported but never merged.
- [ ] You have push rights, and rights to delete branches.
- [ ] Branch protection rules on branches you intend to delete are lifted.
- [ ] The default branch has been retargeted to `master`, or you accept that the
      old default cannot be deleted.
- [ ] You have run dry-run and read the reports.
- [ ] CI that keys off `feature/*` branch names has been checked — those branches
      are about to stop existing.

## Postconditions

- [ ] `git ls-remote --heads origin` lists exactly `master`.
- [ ] `master` contains every merged branch's history (`git log --graph --oneline`).
- [ ] A `pre-delete-backup/<branch>/<timestamp>` tag exists on origin for every
      deleted branch.
- [ ] The repository's default branch on GitHub is `master`.
- [ ] CI passes on `master`.
- [ ] Every developer has run `git fetch --all --prune`; their stale local branches
      are gone.

---

## Summary report template

### Per repository

```markdown
# <repo>

- Path: `<path>`
- Mode: `<dry-run|execute>`  |  Timestamp: `<yyyyMMdd-HHmmss>`
- Halted by conflict: **<yes|no>**

## Branches evaluated (<n>)
## Merged into master (<n>)
## Conflicts (<n>) — require manual resolution
## Backup tags created (<n>)
## Branches deleted (<n>)
## Deletion refused (<n>) — protected or default branch
## Local-only branches, NOT merged (<n>)
## Final master commit
```

### Consolidated

```markdown
# Branch consolidation — consolidated report

- Timestamp / Mode / Delete remote branches / Protected

## <repo>
- Evaluated: <n> | Merged: <n> | Conflicts: <n> | Tagged: <n> | Deleted: <n>
- Halted: <yes|no>
- Final master: `<sha> <subject>`
- Report: `<path>`
```

---

## Recovering a deleted branch

```bash
git fetch origin --tags
git checkout -b <branch> pre-delete-backup/<branch>/<timestamp>
git push -u origin <branch>
```
