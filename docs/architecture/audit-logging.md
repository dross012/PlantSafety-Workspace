# Audit Logging (PlantSafety-specific detail)

**Date:** 2026-07-27

This document covers PlantSafety-specific detail about audit logging: which
PlantSafety operations write audit entries, which are known to be missing
them, and how PlantSafety relates to the shared `Shared.AuditLogging` module
that now owns the C# read contract.

**This document does not duplicate `Shared.AuditLogging`'s own README.**
For the module's layout, consumers, safety invariants, and full extraction
history, see the canonical doc:
`C:\Users\dross\source\Shared\AuditLogging\docs\README.md`.

## 1. Field definitions — `dbo.AuditLog`

The table lives in `[PlantSafety.AuditDB].dbo.AuditLog` (a single, shared,
cross-database, server-level, append-only table — **not** PlantSafety-tenant
data; see §3). Its canonical, idempotent creation script is
`Shared\AuditLogging\sql\Phase0-Tables\00-create-auditlog-table.sql`. Exact
column list, in order, confirmed from that script:

| Column | Type | Nullable | Notes |
|---|---|---|---|
| `TableName` | `nvarchar(128)` | No | Name of the table/entity the write affected |
| `SourceDatabase` | `nvarchar(128)` | No | `DB_NAME()` of the writing database (PlantSafety.CoreDB, ArchiveDB, CloudIdentityDB, PredictiveDB, ReportingDB, IntegrationDB, DiagnosticsDB, ExportImportDB) |
| `Operation` | `nvarchar(50)` | No | e.g. `Insert`, `Update`, `Delete`, `Archive` |
| `ChangedBy` | `nvarchar(128)` | No | `SYSTEM_USER` or resolved application identity |
| `ChangeTime` | `datetime2` | No | UTC timestamp of the write; the table's only ordering/paging key (see below) |
| `ViewerAccessed` | `bit` | No, default `0` | Only ever inserted as 0/1 |
| `ErrorContext` | `nvarchar(400)` | Yes | Free-text; populated by catch-block writes, matches `usp_SearchAuditLog`'s `@SearchText` `LIKE` filter |
| `Severity` | `int` | No, default `0` | Matches `ERROR_SEVERITY()` on catch-block writes |
| `Module` | `nvarchar(128)` | No | Producing module/stored-procedure name |
| `BeforeSnapshot` | `nvarchar(max)` | Yes | Reserved; no confirmed non-NULL write exists anywhere in the codebase today |
| `AfterSnapshot` | `nvarchar(max)` | Yes | Reserved; same as above |

Two deliberate deviations from this solution's usual table conventions,
carried over unchanged from `Shared.AuditLogging`'s own findings (see that
module's README for the full evidence trail):

- **No primary key.** No identity/PK column is evidenced in any of the ~400
  confirmed `INSERT` call sites scanned during the extraction.
- **No canonical metadata columns** (`CreatedUtc`/`CreatedBy`/`UpdatedUtc`/
  `UpdatedBy`/`IsDeleted`). `ChangedBy`/`ChangeTime` already fill the
  equivalent role, and the table is append-only (no update/delete path).

## 2. Event semantics — what writes an audit entry today, and known gaps

### Coverage (confirmed writers)

Audit writes are **embedded directly inside stored procedures**, not issued
as a separate application-layer call — the write path is: MediatR command
handler → Dapper repository → `EXEC usp_<Verb>` → the procedure itself issues
`INSERT INTO dbo.AuditLog` (via the per-database `dbo.AuditLog` synonym,
see `PlantSafety.Infrastructure/Persistence/Migrations/Phase8-Structural/
83-synonyms-audit-error.sql`) on its success path and/or inside a `CATCH`
block. Roughly 400 stored procedures across
`PlantSafety.Infrastructure/**/Sql/Procedures/*-procedures.sql` follow this
pattern. Based on a prior audit-coverage pass, the following PlantSafety
domains have confirmed audit-log side effects on their mutating stored
procedures:

- **AccidentManagement** — `Accident` create/update/status-transition/archive
  writes.
- **RiskManagement** — `Risk` create/update/status-transition/archive writes.
- **ContinuousImprovement (CorrectiveAction/CAPA)** — `CorrectiveAction`
  create writes (`usp_CorrectiveAction_Create`); CAPA read-side procedures
  (`usp_CAPA_GetActions`) are naturally not audited (reads aren't audited
  anywhere in this solution — only mutations are).
- **RiskManagement ControlMeasure** — control-measure create/update writes.

### Known gaps (unaudited operations)

A prior audit-coverage pass identified specific operational surfaces whose
mutations do **not** currently write to `dbo.AuditLog`, confirmed still true
in the current code:

- **Feature Flag toggles** — `SetFeatureFlagEnabledCommandHandler`
  (`PlantSafety.Application/Operations/Configuration/Handlers/
  SetFeatureFlagEnabledCommandHandler.cs`) records the toggle only via
  `ILogger` (`LogInformation` on enable, `LogWarning` on disable) — it never
  writes to `dbo.AuditLog`. This is an ops kill-switch mutation with no
  durable, queryable audit trail entry, only application logs.
- **Backup triggers** — `PlantSafety.Api/CrossDomain/Backup` (`BackupV1Controller`,
  `BackupV2Controller`) and `PlantSafety.Application/CrossDomain/Backup`
  currently only expose a read path (`GetBackupQuery`/`GetBackupQueryHandler`
  over `IBackupRepository`); no backup-trigger mutation command exists to
  audit in the first place today, and none of the existing Backup code
  writes to `dbo.AuditLog`. Flagged here as a gap for whenever a
  backup-trigger write path is added — it should follow the same
  stored-procedure-embedded audit pattern as every other mutation.

These two gaps are **not fixed by this documentation pass** — they are
recorded here per the "document known gaps rather than the ideal" rule, for a
future engineering pass to close.

## 3. Shared module ownership

- **The C# contract** — `IAuditLogRepository`, `AuditLogEntry`,
  `AuditLogSearchCriteria`, `AuditLogSearchResult` — is owned by
  `Shared.AuditLogging.Abstractions`. The Dapper implementation
  (`AuditLogDapperRepository`, calling `dbo.usp_SearchAuditLog`) and DI wiring
  (`AddSharedAuditLogging`) are owned by `Shared.AuditLogging.Infrastructure`.
  PlantSafety no longer has its own copy of these types — its Audit Viewer UI
  (`PlantSafety.Api/Operations/AuditViewer`,
  `PlantSafety.Application`'s `SearchAuditLogQuery`/`SearchAuditLogQueryHandler`)
  references `Shared.AuditLogging.Abstractions` directly, and
  `PlantSafety.Infrastructure/DependencyInjection/
  InfrastructureServiceRegistration.cs` calls
  `services.AddSharedAuditLogging(configuration)` instead of registering a
  PlantSafety-local repository.
- **The underlying `dbo.AuditLog` table is enterprise-wide shared
  infrastructure, not PlantSafety-owned.** It lives in
  `PlantSafety.AuditDB` for historical/physical reasons (that is where it was
  first found live), but is written to by stored procedures across all 8
  module databases in this solution (PlantSafety.CoreDB, ArchiveDB,
  CloudIdentityDB, PredictiveDB, ReportingDB, IntegrationDB, DiagnosticsDB,
  ExportImportDB), each via its own `dbo.AuditLog` synonym pointing back at
  the one physical table. PlantSafety must not treat this table as something
  it can unilaterally alter — any schema change requires coordinating across
  every module database that writes to it, since `Shared.AuditLogging` is now
  the single canonical read (and, per its `Phase0-Tables` script, definitional)
  owner of the table's shape.

## Related documents

- `Shared\AuditLogging\docs\README.md` — canonical module doc (layout,
  consumers, safety invariants, extraction history).
- `docs\architecture\plantsafety-architecture.md` — how the audit-log side
  effect fits into PlantSafety's overall write flow.
- `Reporting\docs\audit-report-printing.md` — Reporting-side detail on the
  Audit Report's use of `Shared.AuditLogging.Abstractions.IAuditLogRepository`
  against the same shared table.
- `Reporting\docs\safety-reports-integration.md` — the 4 new safety report
  types, which use the sibling `Shared.SafetyData` module rather than
  `Shared.AuditLogging`.
