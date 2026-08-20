# PlantSafety cutover plan: `localhost\SQLEXPRESS` → `localhost\PLANTSAFETY`

## Status

**Planned, not executed (2026-08-19).** This is the concrete procedure for
the actual switch — repointing every connection string and marking the
source read-only. Nothing in this document has been run. It exists so the
cutover can be picked up and executed deliberately, on a chosen date,
rather than improvised.

Prerequisite work already done and verified — see
`docs/sqlexpress-to-plantsafety-backup-restore.md`:
- All 18 `PlantSafety.*` databases backed up and restored onto
  `localhost\PLANTSAFETY`, `DBCC CHECKDB` clean, table/row counts
  exact-matched.
- All 18 databases' ownership is accounted for — no unexplained data.

## Pre-cutover checklist

1. **Confirm no application is running.** As of 2026-08-19, no PlantSafety
   Windows service was found running and nothing was listening on common
   web ports — re-verify this is still true immediately before the window,
   since that assumption is what makes this a data-safety exercise rather
   than a live-traffic cutover.
2. **Take a fresh backup/restore.** The verified copy on `PLANTSAFETY` is a
   point-in-time snapshot from 2026-08-19. Anything written to
   `SQLEXPRESS` since then (manual testing, the daily
   `usp_PlantSafety_BackupFull`/`Diff`/`Log` jobs still targeting the old
   instance, etc.) is not on `PLANTSAFETY` yet. Re-run the same procedure
   documented in `sqlexpress-to-plantsafety-backup-restore.md` — `EXEC
   msdb.dbo.usp_PlantSafety_BackupFull`, verify, `RESTORE ... WITH MOVE,
   REPLACE`, `DBCC CHECKDB` — immediately before starting the window below,
   so the cutover moves current data, not a stale snapshot. `WITH REPLACE`
   is required this time since `PLANTSAFETY` already has these databases.
3. **Pick a window** where nobody is actively using `PlantSafetyPMForm.xlsm`
   or any tool that might write to `CoreDB`/`AuditDB`/`IntegrationDB` via
   one of the standalone import tools below.
4. **Notify whoever owns this repo's day-to-day work** — this document
   doesn't know who that is; that's a step for whoever schedules the actual
   date.

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

1. Confirm the pre-cutover checklist above, including the fresh
   backup/restore with `WITH REPLACE`.
2. Update all of Group A and Group B's connection strings/instance names
   from `SQLEXPRESS`/`(local)\SQLEXPRESS` to `PLANTSAFETY`/
   `localhost\PLANTSAFETY` (match whichever host-name style — `localhost`
   vs `(local)` — each file already uses, for a minimal diff).
3. Update Group C and Group D's default fallback strings the same way.
4. Update Group E's `.ps1` scripts, then re-run the `Register-*.ps1`
   scripts so the already-registered Scheduled Tasks pick up the new
   instance (or manually verify/update the Scheduled Task definitions —
   check both paths, don't assume one alone is sufficient).
5. Build and smoke-test `PlantSafety.Api` and `Reporting.Api` against
   `localhost\PLANTSAFETY` before calling the cutover done — confirm their
   health checks pass and a read-only operation against each succeeds.
6. Run one of the Group E scripts manually
   (`Invoke-PlantSafetyBackup.ps1 -BackupType Full`) against
   `localhost\PLANTSAFETY` to confirm the repointed automation actually
   works end to end, not just that it compiles.
7. Only after 5 and 6 both pass: set `localhost\SQLEXPRESS`'s
   `PlantSafety.*` databases `READ_ONLY` (`ALTER DATABASE ... SET
   READ_ONLY WITH ROLLBACK IMMEDIATE`) — reversible via `SET READ_WRITE`,
   not a drop. This is the point of no casual return; everything before it
   is safely reversible by just reverting the config changes.

## Rollback

Before step 7: revert the config/script changes (steps 2–4), nothing on
either instance was touched destructively, so rollback is trivial.

After step 7: `ALTER DATABASE ... SET READ_WRITE` on the `SQLEXPRESS`
copies, then revert the config/script changes back. Data written to
`PLANTSAFETY` between cutover and the decision to roll back would need to
be reconciled or accepted as lost — this is the one point in the whole
plan with genuine risk, which is why step 7 is deliberately the last step
and gated on 5 and 6 both passing first.

## Post-cutover verification

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
