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
- **Initial pass found a live connection string for only 3 of 18
  databases** (`PlantSafety.Api` → `CoreDB` + `IntegrationDB`;
  `Reporting.Api` → `AuditDB` + `IntegrationDB`), because it searched
  `appsettings*.json` and git-tracked files only. A follow-up pass
  searching the full working tree — including the untracked
  `PlantSafety.Infrastructure/Operations/{Archival,Monitoring,SchemaGuard,
  Configuration}` module, which hardcodes per-database connection strings
  directly in C# Options classes rather than `appsettings.json` — found 7
  more databases genuinely owned by that cross-cutting governance tooling
  (schema-drift guard, archival/purge, error-log viewer, monitoring). See
  "Corrected database ownership" below for the full accounting; only
  `ExportImportDB`'s actual data source remains a genuine open question.
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

At the time the backup/restore below was executed, most of the 18
databases had no traceable owner in this codebase, so a full cutover was
judged too risky without further discovery. A follow-up ownership pass
(2026-08-19, same day) closed most of that gap — see "Corrected database
ownership" — but cutover still has not happened; the backup-and-restore
verification below remains as far as this work has gone.

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

## Corrected database ownership (2026-08-19, follow-up pass)

The original "16 unaccounted databases" framing above was too pessimistic
— it came from a pass that searched `appsettings*.json` and git-tracked
files only. A follow-up pass searched the actual working tree, including
everything currently untracked in git, and found a real owner for 7 more:

| Database | Status | Owner |
|---|---|---|
| `CoreDB` | Owned | `PlantSafety.Api` (`appsettings.json`) |
| `AuditDB` | Owned | `Reporting.Api` (`appsettings.json`) |
| `IntegrationDB` | Owned | `PlantSafety.Api` + `Reporting.Api` (`appsettings.json`) |
| `ArchiveDB` | Owned | `PlantSafety.Infrastructure/Operations/Archival` — `Archival.ArchivalJob`/`TierPolicy`, `[System].[FeatureFlag]` etc., via `ArchivalDapperRepository`/`FeatureFlagDapperRepository` (untracked) |
| `CloudIdentityDB` | Owned (governance-only) | `Operations/Monitoring` + `Operations/SchemaGuard` (Query Store + `[System].[SchemaMigrations]` tracking); no dedicated business module claims it — `Azure/Identity.Infrastructure` is Azure AD/Graph-based, no SQL connectivity |
| `DiagnosticsDB` | Owned | `Operations/{Archival,Monitoring,SchemaGuard}`, targeting `Diagnostics.sp_DiagnosticEvent_Purge` etc. Note: the standalone `Diagnostics/Safety.Diagnostics` engine and `Diagnostics.UI` have **no SQL connectivity at all** — only the Ops module connects |
| `ErrorDB` | Owned | `Operations/{Archival,Monitoring}` (`ErrorDbConnectionString`), queried directly in `MonitoringDapperRepository.cs`, surfaced via `OperationsMonitoringV1Controller`/UI. Also the shared cross-DB error-log sink every stored proc's `CATCH` block writes to |
| `ExportImportDB` | **Owned, but with an open question** | `Operations/{Monitoring,SchemaGuard}` has a connection string, but the dedicated `ExportImport` module (full Domain/Application/Infrastructure/Integration stack, `ExportImportDbOptions.cs`) has an **empty default connection string, no `appsettings.json`, no host, no `Program.cs`** — `AddExportImportInfrastructure()` is only ever called from its own test project. Its 12 tables / 15 rows were populated by *something*, but nothing traceable in this repo is currently doing it. **Follow-up needed before this one is touched in any migration.** |
| `PredictiveDB` | Owned (governance-only) | `Operations/{Monitoring,SchemaGuard}`. The separate `Intelligence` (ML/predictive) module connects to its own `IntelligenceDev` localdb, not this database — no overlap |
| `ReportingDB` | Owned (governance-only) | `Operations/{Monitoring,SchemaGuard}`. **Not** the same database `Reporting.Api` uses — despite the name, `Reporting.Api` connects to `AuditDB`+`IntegrationDB` only |
| `MaintenanceDB` | Deliberately empty placeholder | Documented in untracked `PlantSafety/docs/database/placeholder-databases.md`: "do not populate without a corresponding Domain-layer module" |
| `NotificationsDB` | Deliberately empty placeholder | Same doc |
| `ProductionDB` | Deliberately empty placeholder | Same doc |
| `RoutingDB` | Deliberately empty placeholder | Same doc |
| `SandboxDB` | Deliberately empty placeholder — **naming-hygiene flag** | Same doc. A database named "Sandbox" sitting in the same estate as `CoreDB` (484 tables/57,025 rows) is worth a governance conversation independent of its emptiness |
| `SchedulingDB` | Deliberately empty placeholder | Same doc |
| `TestHarnessDB` | Deliberately empty placeholder — **naming-hygiene flag** | Same doc. Same concern as `SandboxDB` |
| `ToolingDB` | Deliberately empty placeholder | Same doc. `SafetyTooling/` (Roslyn analyzers/CLI/codegen) is pure dev tooling with no SQL connectivity — doesn't own this DB despite the name similarity |

All 18 databases, including the 8 empty placeholders, are backed up
indiscriminately by `usp_PlantSafety_BackupFull`/`Diff`/`Log` (driven off
`sys.databases WHERE name LIKE 'PlantSafety.%'`) — that's infrastructure
coverage, not application ownership, and doesn't change any row above.

**Net effect:** 17 of 18 databases are now accounted for (3 app-owned + 7
governance-owned + 8 deliberately empty). `ExportImportDB` is the one
genuine open item — real data, no traceable writer — and should be
resolved before it's included in any cutover decision.

## What is explicitly NOT done

- **`PlantSafety.Api` / `Reporting.Api` connection strings are unchanged**
  — still pointed at `localhost\SQLEXPRESS`.
- **`localhost\SQLEXPRESS`'s databases are not `READ_ONLY`** and nothing
  was dropped. Unlike Training's own move (where the source was marked
  `READ_ONLY` after cutover), no cutover has happened here, so there is
  nothing to protect yet.
- **`ExportImportDB`'s actual data source has not been identified** — see
  above. This is the one remaining ownership gap.
- **No decommission timeline for `localhost\SQLEXPRESS`.** That instance
  cannot be considered for decommissioning until an actual cutover happens
  and soaks, which has not been scheduled.
