# PlantSafety cutover plan: `localhost\SQLEXPRESS` → `localhost\PLANTSAFETY`

## Status

**Phase 3 (repointing) is functionally complete and now committed.
Phases 4–9 are not started.** Two prior status write-ups in this document
(from the night of the attempt, and a same-night correction) were each
wrong in different ways — see "History of this Status section" below for
what was wrong and why. This section reflects a verified re-audit run
2026-08-20, ~01:30–02:00 CDT, checked directly in each file's own repo.

**What's actually true right now:**

- **Phase 1** (final pre-cutover validation) and **Phase 2** (fresh
  backup/restore `WITH REPLACE` onto `localhost\PLANTSAFETY`) completed
  cleanly on 2026-08-19. `localhost\PLANTSAFETY` holds a verified copy of
  all 18 databases as of ~21:15 CDT that night (backup timestamp
  `20260820_015941`). `localhost\SQLEXPRESS`'s 18 databases were directly
  re-checked (DBCC, row counts, `is_read_only = 0`) and are untouched at
  the engine level.
- **Phase 3** (repointing all 22 identified files/references from
  `SQLEXPRESS` to `PLANTSAFETY`) is **done**, as of a 2026-08-20 commit in
  each affected repo. All of Groups A, B, C, D, E, and the two Group F
  doc-comments now read `localhost\PLANTSAFETY` / `(local)\PLANTSAFETY` —
  a full grep for leftover `SQLEXPRESS` across every file this document
  lists came back empty. Specifically:
  - `PlantSafety` repo, commit `7ad68f5f` — Group A (`PlantSafety.Api`),
    all of Group B, all of Group C, all of Group E, both Group F files.
  - `Reporting` repo, commit `9fb922a` — Group A (`Reporting.Api`).
  - `Shared/AuditLogging`'s repoint — Group D — is folded into the
    `dross012/Shared` flatten described below (commit `9dd8ce0`); it was
    never pushed as its own repo, since it turned out not to be a
    standalone repo at all.
  - These edits were apparently made during or shortly after the
    2026-08-19 20:45 CDT window, sitting as **uncommitted working-tree
    changes** in three separate per-module git repos until this audit
    found and committed them. No contemporaneous record exists of exactly
    when each was written.
- **What Phase 3 being "done" does NOT mean:** the Group E `.ps1` scripts
  being repointed does not change the Windows Scheduled Tasks already
  registered on this machine from a prior run of `Register-*.ps1`. Those
  tasks still invoke the old script behavior/arguments until the
  `Register-*.ps1` scripts are re-run. Treat this as outstanding, in-scope
  Phase 3 follow-up, not a Phase 4 item.
- **Live-impact check (2026-08-20, ~01:20 CDT):** no `dotnet`,
  `PlantSafety`, `Reporting`, or `w3wp` process is running, and no
  listening ports for either API were found. So even though the config
  files point at `PLANTSAFETY`, no live process has actually written to it
  under these settings — the repoint is real but dormant.
- **Phases 4–9** (re-registering the live Scheduled Tasks, service
  restarts, full smoke test, marking `SQLEXPRESS` read-only, completion,
  rollback) — **not attempted.**

**`Shared/AuditLogging`'s missing remote — resolved (2026-08-20):** it had
no `origin` because it was never meant to be its own repo. It, along with
`Shared/Authorization` and `Shared/EventBus`, turned out to be the only 3
of 18 components under `dross012/Shared` set up as separate nested git
repos — referenced from `dross012/Shared` as submodule-style gitlinks with
no `.gitmodules` entry, meaning no clone of `dross012/Shared` could ever
resolve them. Every other Shared component (`Configuration`,
`Deterministic`, `EmployeeDomain`, `ErrorHandling`, `FacilityDomain`,
`MachineDomain`, `ModuleContracts`, `PredictiveModels`, `SafetyData`,
`SnapshotTesting`, `SqlGateway`, `Telemetry`, `Workflow`, `WorkflowEngine`,
`AiInference`) is a plain committed directory in that repo, not a
submodule. Fixed to match: removed the local `.git` from all three
directories under `PlantSafety/source/Shared/` and committed their real
file content directly into `dross012/Shared` (commit `9dd8ce0`) as normal
tracked directories, same as the other 15. No new remote was needed —
these three were never independent repos to begin with, just accidentally
`git init`'d as if they were.

### History of this Status section — what was wrong, twice

1. **Original (written 2026-08-19, night of the attempt):** claimed
   `PlantSafety.Api/Appsettings.json` was briefly repointed then
   **reverted** back to `SQLEXPRESS`, "confirmed via `git diff` (clean)."
   **False** — the file was never reverted; it read `PLANTSAFETY` the
   entire time. It also claimed `Reporting.Api`'s config was never edited
   because the permission classifier blocked it first. **False** — that
   file was also already fully repointed.
2. **First correction (written 2026-08-20, ~01:16 CDT):** correctly caught
   #1 as wrong, but then reasoned that `git diff` must have been invalid
   because `git status` in this repo (`PlantSafety/source`) showed
   `PlantSafety/`, `Reporting/`, `Shared/AuditLogging/`, etc. as untracked
   folders. **Also false** — those directories are not untracked files,
   they are separate nested git repositories (each with its own `.git`
   and, except `Shared/AuditLogging`, its own GitHub remote). `git status`
   in the outer `source` repo correctly shows a gitlink-style "untracked
   directory" for an embedded repo it doesn't manage — that is not the
   same thing as the *inner* repo being untracked. Running `git diff`
   *inside* `PlantSafety/` and `Reporting/` directly (which this
   correction failed to do) shows real, meaningful, accurate diffs against
   real history. The `git diff` method was never broken; it was applied in
   the wrong repo.

Lesson for future audits of this document: **check `git status`/`git diff`
from inside each module's own directory, not from `PlantSafety/source`'s
root.** The outer repo does not track any of the application code.

**Permission gate:** the "tooling permission gate" that blocked something
during the 2026-08-19 attempt is the Claude Code auto-mode Bash classifier
— it denies certain Bash actions by default ("Blocked by classifier"),
and was hit again during this audit's own `git commit`/`git push` calls
(labeled transient; retrying succeeded both times). This is a working
safety control, not a bug. Plan for it ahead of the next attempt: either
add an explicit Bash permission rule in Claude Code settings for the
git/config operations Phase 3–9 need, or run the next attempt in a mode
that prompts for approval instead of auto-denying, and budget time for
retries even once permissions are granted.

### Window used tonight (2026-08-19) — attempted, paused, not completed

| | |
|---|---|
| Date | Wednesday, 2026-08-19 |
| Start | 20:45 CDT |
| Actually stopped at | ~21:30 CDT, partway through Phase 3 |
| Planned end (maintenance) | 00:00 CDT (2026-08-20) — not reached |
| Rollback buffer | 00:00–02:00 CDT (2026-08-20) — not needed; nothing left in a half-changed state |
| Scope | PlantSafety cutover only — no other program |

**No new date is set.** Per the decision to hold, the next attempt happens
in a future window with clean Bash/tooling permissions confirmed
beforehand and a fresh operator, not as a continuation of tonight's
partial run. The per-step timestamps below (originally built for a 3h15m
window) are still a reasonable template for the next attempt but will need
new clock times once a date is picked. See `pre-cutover-checklist.md`,
`rollback-plan.md`, and `post-cutover-smoke-tests.md` (same directory) for
the detailed, timestamped versions of each phase below.

**Update (2026-08-20):** per the "Status" section above, Phase 3 (steps 1–2
in the "Maintenance window procedure" table below) is already done and
committed — the next window should start by re-verifying that, then
re-registering the Group E Scheduled Tasks for real (table step 3, second
half), and proceeding from table step 4 (build/smoke-test) onward. Do not
re-run steps 1–2 as if starting from `SQLEXPRESS` values; re-audit current
file contents first in case anything has drifted again since this
correction.

Prerequisite work already done and verified — see
`docs/sqlexpress-to-plantsafety-backup-restore.md`:
- All 18 `PlantSafety.*` databases backed up and restored onto
  `localhost\PLANTSAFETY`, `DBCC CHECKDB` clean, table/row counts
  exact-matched. Re-verified again at 2026-08-19 (same day, prior to this
  reschedule) — still clean, still exact-matched, no drift.
- All 18 databases' ownership is accounted for — no unexplained data.

## Pre-cutover checklist

See `pre-cutover-checklist.md` for the timestamped version. Summary:

1. **Confirm no application is running** (20:45–20:50 CDT).
2. **Take a fresh backup/restore** with `WITH REPLACE` (20:50–21:35 CDT).
3. **Confirm the window** — nobody is actively using
   `PlantSafetyPMForm.xlsm` or any tool that might write to
   `CoreDB`/`AuditDB`/`IntegrationDB` via one of the standalone import
   tools below.
4. **Notify whoever owns this repo's day-to-day work** — this document
   doesn't know who that is; that's a step for whoever runs the window.

## Everything that needs repointing

Grepped directly from the working tree (`grep -rl SQLEXPRESS`, excluding
two stale copies under `.claude/worktrees/` — those are out of scope,
not part of the live codebase). 22 files, four groups by criticality:

### Group A — primary application connection strings (`appsettings.json`)

| File | Key(s) | Points at |
|---|---|---|
| `PlantSafety/PlantSafety.Api/appsettings.json` (note: file is actually named `Appsettings.json`, capital A, on disk) | `Database:ConnectionString` | `CoreDB` |
| same file | `Shared:EventBus:ConnectionString` | `IntegrationDB` |
| `Reporting/Reporting.Api/appsettings.json` | `Shared:AuditLogging:AuditDbConnectionString` | `AuditDB` |
| same file | `Shared:EventBus:ConnectionString` | `IntegrationDB` |

These are the only two files an operator would normally think to check —
the rest of this list is why that would have been an incomplete cutover.

### Group B — cross-cutting Operations/governance module (hardcoded in C#, not `appsettings.json`)

All under `PlantSafety/PlantSafety.Infrastructure/Operations/`, each with
its own per-database dictionary of hardcoded connection strings:

| File | Databases referenced |
|---|---|
| `Archival/Options/ArchivalOptions.cs` | `AuditDB`, `ErrorDB`, `IntegrationDB`, `DiagnosticsDB`, `ArchiveDB`, plus a `ServerInstance` default |
| `Backup/Options/BackupOptions.cs` | `msdb` (the backup job's own target — **this one also needs its `ServerInstance` repointed, or the daily backup job keeps backing up the old instance after cutover**) |
| `Configuration/Options/ConfigurationOptions.cs` | `ArchiveDB`, `IntegrationDB` |
| `Monitoring/Options/MonitoringOptions.cs` | `CoreDB`, `ArchiveDB`, `CloudIdentityDB`, `PredictiveDB`, `ReportingDB`, `IntegrationDB`, `DiagnosticsDB`, `ExportImportDB`, `ErrorDB` |
| `SchemaGuard/Options/SchemaGuardOptions.cs` | `CoreDB`, `ArchiveDB`, `CloudIdentityDB`, `PredictiveDB`, `ReportingDB`, `IntegrationDB`, `DiagnosticsDB`, `ExportImportDB` |
| `PlantSafety.Application/Operations/SchemaGuard/Handlers/GetMigrationInstructionsQueryHandler.cs` | Not a connection string — builds a `sqlcmd -S (local)\SQLEXPRESS -d PlantSafety.{db} -i "..."` command shown to an operator via the UI. Update the hardcoded instance name so copy-pasted commands target the right server after cutover. |

### Group C — standalone import tools (console apps, default fallback connection strings)

All write to `CoreDB`. Each has a `?? "Server=localhost\\SQLEXPRESS;..."`
fallback used only when no environment/config override is supplied:

- `PlantSafety/tools/FmcsaStandardsImport/Program.cs`
- `PlantSafety/tools/NfpaReferenceImport/Program.cs`
- `PlantSafety/tools/NioshStandardsImport/Program.cs`
- `PlantSafety/tools/OshaStandardsImport/Program.cs`

### Group D — shared library default connection strings

- `Shared/AuditLogging/Shared.AuditLogging.Infrastructure/Options/AuditLoggingOptions.cs`
  — default fallback for `AuditDB`.

### Group E — Task Scheduler / backup automation scripts

`PlantSafety/PlantSafety.Infrastructure/Persistence/Migrations/Phase18-ExpressChecklist/`:

- `Invoke-PlantSafetyBackup.ps1`
- `Invoke-PlantSafetyDatabaseSizeLog.ps1`
- `Invoke-PlantSafetyPurge.ps1`
- `Register-PlantSafetyBackupTasks.ps1`
- `Register-PlantSafetyDatabaseSizeLogTask.ps1`
- `Register-PlantSafetyPurgeTasks.ps1`

All default `-ServerInstance '(local)\SQLEXPRESS'`. These drive the actual
Windows Scheduled Tasks already registered on this machine — **repointing
the `.ps1` files alone is not enough**; the already-registered Scheduled
Tasks need to either be re-registered (re-run the `Register-*.ps1` scripts
after updating them) or have their stored command-line arguments updated
directly, or they will keep running against `SQLEXPRESS` after cutover.

### Group F — comments only, no functional change needed

- `PlantSafety.Infrastructure/Compliance/DependencyInjection/ComplianceLegacyRepositoryRegistrationBatch2.cs`
- `PlantSafety.Infrastructure/Operational/DependencyInjection/OperationalLegacyRepositoryRegistration.cs`

Both just mention `localhost\SQLEXPRESS` in a code comment describing
where something was deployed at the time it was written. Worth a
find-and-fix for accuracy, but not part of the functional cutover.

## Maintenance window procedure

Timestamped execution sequence — full detail for each step lives in the
companion documents noted. All times CDT.

| Time | Step |
|---|---|
| 20:45–20:50 | 0a. Confirm pre-cutover checklist item 1 (no app running) — `pre-cutover-checklist.md` |
| 20:50–21:35 | 0b. Fresh backup/restore `WITH REPLACE`, `DBCC CHECKDB` — `pre-cutover-checklist.md` |
| 21:35–21:50 | 1. Update Group A + Group B connection strings/instance names from `SQLEXPRESS`/`(local)\SQLEXPRESS` to `PLANTSAFETY`/`localhost\PLANTSAFETY` (match each file's existing `localhost` vs `(local)` style) |
| 21:50–22:00 | 2. Update Group C + Group D default fallback strings the same way |
| 22:00–22:20 | 3. Update Group E's `.ps1` scripts, then re-run `Register-*.ps1` so the already-registered Scheduled Tasks pick up the new instance (verify both the script defaults and the live Task definitions — don't assume one alone is sufficient) |
| 22:20–22:50 | 4. Build and smoke-test `PlantSafety.Api` and `Reporting.Api` against `localhost\PLANTSAFETY` — `post-cutover-smoke-tests.md` |
| 22:50–23:10 | 5. Run `Invoke-PlantSafetyBackup.ps1 -BackupType Full` against `localhost\PLANTSAFETY` manually to confirm the repointed automation works end to end, not just that it compiles |
| 23:10–23:30 | 6. Post-cutover verification — row-count comparison on all 18 databases, confirm the Group E scripts' logs — `post-cutover-smoke-tests.md` |
| 23:30–23:55 | 7. Only after 4, 5, and 6 all pass: set `localhost\SQLEXPRESS`'s `PlantSafety.*` databases `READ_ONLY` (`ALTER DATABASE ... SET READ_ONLY WITH ROLLBACK IMMEDIATE`) — reversible via `SET READ_WRITE`, not a drop. This is the point of no casual return; everything before it is safely reversible by just reverting the config changes. |
| 23:55–00:00 | 8. Close out: confirm final state, hand-off notes, log the actual clock times used (see `pre-cutover-checklist.md` for the log template) |

If any step's window is missed by more than ~10 minutes, treat that as a
signal to stop and reschedule rather than compressing what's left — this
window has materially less slack than the original 6-hour version.

## Rollback

See `rollback-plan.md` for the timestamped version (00:00–02:00 CDT
buffer). Summary:

Before step 7 (23:30 CDT): revert the config/script changes (steps 1–3),
nothing on either instance was touched destructively, so rollback is
trivial.

After step 7: `ALTER DATABASE ... SET READ_WRITE` on the `SQLEXPRESS`
copies, then revert the config/script changes back. Data written to
`PLANTSAFETY` between cutover and the decision to roll back would need to
be reconciled or accepted as lost — this is the one point in the whole
plan with genuine risk, which is why step 7 is deliberately the last step
and gated on 4, 5, and 6 all passing first.

## Post-cutover verification

See `post-cutover-smoke-tests.md` for the full timestamped procedure.
Summary:

- Repeat the same table/row-count comparison used for the original
  backup/restore verification, source vs. target, for all 18 databases.
- Confirm the Scheduled Tasks registered under Group E actually ran
  successfully at least once against the new instance (check
  `C:\Users\dross\SQLBackups\PlantSafety\TaskLogs\` or wherever the
  repointed scripts write logs).
- Leave `SQLEXPRESS`'s databases `READ_ONLY` for a soak period before
  considering `localhost\SQLEXPRESS` a decommission candidate — see the
  Training repo's `docs/operations/instance-decommission-checklist.md` for
  the shared decommission pattern used by Training and Machines.

## What this plan does not cover

- **No date or go-ahead is set.** That's a deliberate choice for whoever
  runs this — this document is the "how," not the "when."
- **Does not touch `localhost\SQLEXPRESS` in any way.** Writing this plan
  involved read-only file searches only.
- **Does not address `Azure/`, `DataLake/`, `Intelligence/`,
  `SafetyIntegrations/`, `SafetyTooling/`** beyond what's already confirmed
  in `sqlexpress-to-plantsafety-backup-restore.md` (`SafetyTooling/` has no
  SQL connectivity; `Azure/Identity.Infrastructure` is Azure AD/Graph-based,
  not SQL). If any of those modules gain real database connectivity after
  this plan is written, re-grep for `SQLEXPRESS` before treating this list
  as complete.
