# `localhost\SQLEXPRESS` → `localhost\PLANTSAFETY` — backup and restore verification

## Status

**Verified copy exists (2026-08-19). Cutover has NOT happened.** All 18
`PlantSafety.*` databases were backed up from `localhost\SQLEXPRESS` and
restored onto a new dedicated `localhost\PLANTSAFETY` instance as a
non-destructive, reversible first step. `localhost\SQLEXPRESS` itself was
not touched beyond taking the backup — no `READ_ONLY` marking, no
connection-string changes, nothing dropped. `PlantSafety.Api` and
`Reporting.Api` still point at `localhost\SQLEXPRESS`.

## Why this exists

Prompted by the Training program moving its own databases off a shared
scratch instance (`localhost\SQLEXPRESS01`) onto its own dedicated
instance, which raised the same question for every program still sharing
an instance — including PlantSafety's use of `localhost\SQLEXPRESS`, which
this repo's own conventions (and the sibling Training/Machines repos)
flag as hosting live production data. See the Training repo's
`docs/operations/instance-consolidation-runbook.md` for the cross-program
picture.

## What was found before acting

- **The `Sql Server Express PlantSafety.docx` prompt does not describe this
  migration.** It asks for a small greenfield `PlantSafetyDB` (~8 tables:
  `SafetyAlert`, `SafetyIncident`, `SafetyRule`, `SafetyTask`,
  `SafetyEquipment`, `SafetyNotes`, `SafetyUpdate`, two summary tables) —
  unrelated to the 18-database estate this note covers.
- **No migration runbook or deviations register already existed** for this
  question in this repo (unlike Training's `docs/deviations.md`).
- **Only 2 of 18 databases have a traceable live connection string**:
  `PlantSafety.Api` → `CoreDB` + `IntegrationDB`; `Reporting.Api` →
  `AuditDB` + `IntegrationDB`. The other 16 database names have no
  discoverable code reference anywhere in this repo, tracked or untracked.
  8 of those 16 turned out to be genuinely empty (0 tables) on inspection —
  see the table below.
- **No application service was found running** on this machine at the time
  of this check (no matching Windows service, nothing listening on common
  web ports) — there was no live traffic to coordinate a no-downtime
  cutover around.
- **Real backup/DR tooling already existed and was reused as-is**:
  `PlantSafety.Infrastructure/Persistence/Migrations/Phase18-ExpressChecklist/
  Invoke-PlantSafetyBackup.ps1`, calling the pre-existing
  `msdb.dbo.usp_PlantSafety_BackupFull` procedure. This note's backup was
  taken by calling that same procedure directly
  (`EXEC msdb.dbo.usp_PlantSafety_BackupFull @BackupRootPath = ...`) rather
  than writing new backup logic.

Given the above — most of the 18 databases have no traceable owner in this
codebase — a full cutover was judged too risky to execute without further
discovery. The backup-and-restore verification below is as far as this
pass went.

## What was done

1. `EXEC msdb.dbo.usp_PlantSafety_BackupFull @BackupRootPath =
   N'D:\Manufacturing Program\Backups\PlantSafety\Full\'` against
   `localhost\SQLEXPRESS` — backed up all 18 `PlantSafety.*` databases in
   one call via the existing cursor-driven procedure. Output path chosen
   for one reason: SQL Server Express instances on this machine each run
   as their own isolated virtual service account
   (`NT SERVICE\MSSQL$<instance>`), so the backup location has to be
   readable/writable by both `SQLEXPRESS`'s and `PLANTSAFETY`'s service
   accounts — a shared folder under `D:\Manufacturing Program\Backups\`,
   not either instance's own default backup directory.
2. `RESTORE VERIFYONLY ... WITH CHECKSUM` on every `.bak` file before
   touching the target.
3. `RESTORE FILELISTONLY` against `localhost\PLANTSAFETY` to discover each
   backup's logical file names (several databases have multiple data files
   — e.g. `CoreDB` has `Core.ColdData`/`Core.HotData`/`Core.Index`/
   `Core.ReadOnly` alongside its primary file), then `RESTORE DATABASE ...
   WITH MOVE` for every logical file to that instance's own default data
   path.
4. `DBCC CHECKDB (..., ) WITH NO_INFOMSGS, ALL_ERRORMSGS` on all 18
   restored databases — clean on every one.
5. Compared table count and total row count per database, source vs.
   target — exact match on all 18.

## Verification results

| Database | Tables | Rows | `DBCC CHECKDB` |
|---|---:|---:|---|
| `CoreDB` | 484 | 57,025 | clean |
| `AuditDB` | 3 | 5,408 | clean |
| `IntegrationDB` | 17 | 3,220 | clean |
| `ArchiveDB` | 5 | 66 | clean |
| `CloudIdentityDB` | 5 | 8 | clean |
| `DiagnosticsDB` | 16 | 20 | clean |
| `ErrorDB` | 1 | 12 | clean |
| `ExportImportDB` | 12 | 15 | clean |
| `PredictiveDB` | 4 | 9 | clean |
| `ReportingDB` | 7 | 11 | clean |
| `MaintenanceDB` | 0 | 0 | clean (empty shell) |
| `NotificationsDB` | 0 | 0 | clean (empty shell) |
| `ProductionDB` | 0 | 0 | clean (empty shell) |
| `RoutingDB` | 0 | 0 | clean (empty shell) |
| `SandboxDB` | 0 | 0 | clean (empty shell) |
| `SchedulingDB` | 0 | 0 | clean (empty shell) |
| `TestHarnessDB` | 0 | 0 | clean (empty shell) |
| `ToolingDB` | 0 | 0 | clean (empty shell) |

`CoreDB`'s 484 tables / 57,025 rows confirms it is in genuine active use,
consistent with `PlantSafety.Api`'s live connection to it. The eight
zero-row databases were already empty on the source — not a restore defect.

## What is explicitly NOT done

- **`PlantSafety.Api` / `Reporting.Api` connection strings are unchanged**
  — still pointed at `localhost\SQLEXPRESS`.
- **`localhost\SQLEXPRESS`'s databases are not `READ_ONLY`** and nothing
  was dropped. Unlike Training's own move (where the source was marked
  `READ_ONLY` after cutover), no cutover has happened here, so there is
  nothing to protect yet.
- **The 16 databases with no traceable code owner have not been
  investigated further.** Before any real cutover, something needs to
  positively confirm what — if anything — reads or writes
  `ProductionDB`/`SchedulingDB`/`MaintenanceDB`/`DiagnosticsDB`/
  `PredictiveDB`/`RoutingDB`/`ToolingDB`/`NotificationsDB`/
  `ExportImportDB`/`CloudIdentityDB`/`ArchiveDB`/`SandboxDB`/
  `TestHarnessDB`/`ErrorDB` beyond the backup job itself. The untracked
  working-tree folders present in this repo at the time of this check
  (`Azure/`, `DataLake/`, `Diagnostics/`, `ExportImport/`, `Intelligence/`,
  `PlantSafety/`, `Reporting/`, `SafetyIntegrations/`, `SafetyTooling/`,
  and parts of `Shared/`) were not committed and were not searched by this
  pass beyond what a working-tree grep could see — a plan built only from
  git history would miss most of the application code that could touch
  these databases.
- **No decommission timeline for `localhost\SQLEXPRESS`.** That instance
  cannot be considered for decommissioning until an actual cutover happens
  and soaks, which has not been scheduled.
