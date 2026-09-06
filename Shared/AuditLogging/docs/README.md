# Shared.AuditLogging

## What this module is

`Shared.AuditLogging` is the single canonical **read path** for
`[PlantSafety.AuditDB].dbo.AuditLog` — a shared, cross-database, server-level,
append-only audit table written to by stored procedures across all 8 module
databases (PlantSafety.CoreDB, ArchiveDB, CloudIdentityDB, PredictiveDB,
ReportingDB, IntegrationDB, DiagnosticsDB, ExportImportDB) via a per-database
`dbo.AuditLog` synonym. See `PlantSafety/PlantSafety.Infrastructure/Persistence/
Migrations/Phase8-Structural/83-synonyms-audit-error.sql` for the synonym setup
and the table's origin.

`dbo.AuditLog` is **not** PlantSafety-tenant data. PlantSafety just happened to
be the first (and, until this extraction, only) module with a repository over
it. This module exists so any module that needs to read the platform audit
trail — today PlantSafety's Audit Viewer UI and Reporting's Audit Report PDF —
depends on one shared, versioned contract instead of either duplicating the
query logic or taking a forbidden cross-module dependency on PlantSafety.

## Why it exists (history)

1. `PlantSafety.Application`/`PlantSafety.Infrastructure` originally owned
   `IAuditLogRepository` / `AuditLogDapperRepository` under
   `Operations/AuditViewer/*` (created 2026-07-20), reading via inline Dapper
   SQL text against `PlantSafety.AuditDB`.
2. `Reporting`'s Audit Report PDF (`Reporting.Infrastructure/DataSources/Audit/
   AuditDataSourceProvider.cs`) needed the same data, but Reporting must not
   depend on PlantSafety (module boundary rule) — so it shipped 2026-07-26 as a
   stub returning `AuditTrailData.Empty` with a TODO.
3. This module (`Shared.AuditLogging`) was created 2026-07-26 to break that
   impasse: the read logic was extracted out of PlantSafety into a shared
   module that both PlantSafety and Reporting reference.

## Layout

- **`Shared.AuditLogging.Abstractions`** (net8.0, no external package
  dependencies) — `AuditLogEntry`, `AuditLogSearchCriteria`,
  `AuditLogSearchResult`, `IAuditLogRepository`. Safe for both a tenant-scoped
  application layer (PlantSafety.Application) and a report generator
  (Reporting.Infrastructure) to reference directly.
- **`Shared.AuditLogging.Infrastructure`** (net8.0) — `AuditLogDapperRepository`
  (Dapper, calls `dbo.usp_SearchAuditLog`), `AuditLoggingOptions` (bound from
  configuration section `Shared:AuditLogging`), `AuditLogQueryException`, and
  `ServiceCollectionExtensions.AddSharedAuditLogging(...)` for DI registration.
- **`Shared.AuditLogging.Tests`** — unit tests for `AuditLogDapperRepository`
  and the DI extension, mirroring the coverage PlantSafety.Tests originally had
  for the pre-extraction repository.
- **`sql/Phase1-StoredProcedures/01-usp-search-auditlog.sql`** — creates
  `dbo.usp_SearchAuditLog` against `[PlantSafety.AuditDB]`. See that file's
  header comment for why the script lives here rather than as another
  PlantSafety-numbered migration phase, and why the procedure returns two
  result sets (total count, then page) instead of an `OUTPUT` parameter.
- **`sql/Phase0-Tables/00-create-auditlog-table.sql`** — canonical,
  idempotent `CREATE TABLE` script for `[PlantSafety.AuditDB].dbo.AuditLog`
  itself, added 2026-07-26 as a follow-up extraction (checklist items 1 & 6).
  No PlantSafety-local `CREATE TABLE` script for this table was ever found
  in the repo — see that file's header comment for the full evidence trail
  and the reverse-engineered column list/types. Also deliberately does NOT
  add a StrongId PK or the canonical metadata columns
  (`CreatedUtc`/`CreatedBy`/`UpdatedUtc`/`UpdatedBy`/`IsDeleted`); see that
  file's "NO PRIMARY KEY" / "NO CANONICAL METADATA COLUMNS" notes.
- **`sql/Phase0-Tables/01-recommended-indexes.sql`** — optional, additive,
  not-yet-verified-against-production indexes recommended to support
  `usp_SearchAuditLog`'s filter/paging query shape. Not applied to any live
  database by this extraction; flagged for DBA review first.

## Consumers

- **PlantSafety** (`PlantSafety.Application`/`PlantSafety.Infrastructure`/
  `PlantSafety.Api`) — the Audit Viewer UI's `SearchAuditLogQuery`/
  `SearchAuditLogQueryHandler`/`OperationsAuditViewerV1Controller` now use
  `Shared.AuditLogging.Abstractions` types directly instead of PlantSafety-local
  copies. DI registration moved from `InfrastructureServiceRegistration.cs`'s
  `services.Configure<AuditViewerOptions>(...)` /
  `services.AddScoped<IAuditLogRepository, AuditLogDapperRepository>()` to a
  single `services.AddSharedAuditLogging(configuration)` call.
- **Reporting** (`Reporting.Infrastructure`) — `AuditDataSourceProvider` now
  injects `Shared.AuditLogging.Abstractions.IAuditLogRepository`, calls
  `SearchAsync` in real time (no caching, no ETL — same as PlantSafety's
  viewer), and maps `AuditLogEntry` → `Reporting.Domain.ValueObjects.
  AuditLogEntryData`/`AuditTrailData`. See `Reporting/docs/audit-report-
  printing.md`.

Both consumers bind `AuditLoggingOptions` (`Shared:AuditLogging` configuration
section) to the **same physical database** — `PlantSafety.AuditDB` — since it
is one shared table, not two.

## Safety invariants (carried forward from the original PlantSafety files)

- **Read-only.** Nothing in this module may INSERT, UPDATE, or DELETE against
  `dbo.AuditLog`. `dbo.usp_SearchAuditLog` is SELECT-only. This must never
  change — `dbo.AuditLog` is append-only audit infrastructure written
  exclusively by the canonical stored-procedure template's best-effort audit
  block in each module database.
- **No invented primary key.** No identity/primary-key column is evidenced in
  the scanned migration scripts for `dbo.AuditLog`, so none is invented here.
  Callers page/order by `ChangeTime`.
- **Real-time only.** No caching, ETL, or projection tables — every
  `SearchAsync` call queries the live table via the stored procedure.
- **SQL injection discipline.** Every filter value is bound as a stored
  procedure parameter; nothing is ever concatenated into SQL text.
- **No EF Core.** Dapper + stored procedure only, matching every other module
  in this solution.

## Judgment calls made during extraction (2026-07-26)

- **Type naming**: `AuditLogEntryDto` → `AuditLogEntry` (explicit instruction).
  `AuditLogSearchResultDto` → `AuditLogSearchResult` (dropped the `Dto` suffix
  for consistency with `AuditLogEntry`, since this is now a shared,
  cross-module contract rather than a single layer's transport DTO).
  `AuditLogSearchCriteria` kept its name (was never `Dto`-suffixed).
- **Error handling**: the original PlantSafety repository routed
  `SqlException` through `PlantSafety.Infrastructure.Common.Exceptions.
  ErrorClassifier`, which maps into PlantSafety's own exception taxonomy
  (`PlantSafety.Application.Common.Errors`, `PlantSafety.SharedKernel`).
  `Shared.AuditLogging` cannot depend on PlantSafety assemblies without
  inverting the dependency direction this extraction exists to fix, so it
  defines its own minimal `AuditLogQueryException` instead. Consumers that want
  `ErrorClassifier`-style mapping onto their own taxonomy catch
  `AuditLogQueryException` and re-classify at their layer.
- **Stored procedure pagination shape**: no existing paginated stored procedure
  in the PlantSafety migration scripts returns a total row count (the `*_List`
  procedures examined return only the page). Since AuditLog search callers need
  a total for page-count UI, `dbo.usp_SearchAuditLog` returns two result sets
  (count, then page) rather than adding an `OUTPUT` parameter — see the
  script's header comment for the full rationale.
- **SQL script location**: placed under `Shared/AuditLogging/sql/` rather than
  as a new PlantSafety `Phase10+` migration file, since `dbo.AuditLog` is
  explicitly documented as not PlantSafety-owned.
- **DI extension naming**: `AddSharedAuditLogging(IServiceCollection,
  IConfiguration)`, mirroring the single `Add<Module>` extension pattern used
  elsewhere in the solution (e.g. Reporting.Infrastructure's
  `DataSourceServiceCollectionExtensions`).
- **Logging**: uses `ILogger<T>` (Microsoft.Extensions.Logging.Abstractions)
  injection, matching the house pattern where Serilog is wired as the
  `ILogger` backend via `Log.Logger`/`UseSerilog()` in `PlantSafety.Api/
  Program.cs` and `PlantSafety.Infrastructure` — no direct `Serilog.Log` static
  calls.

## Table-definition follow-up (2026-07-26, checklist items 1 & 6)

A second extraction pass added `sql/Phase0-Tables/00-create-auditlog-table.sql`
and `sql/Phase0-Tables/01-recommended-indexes.sql`. Notes for whoever does the
next step (code cutover / eventual PlantSafety-local cleanup):

- **No PlantSafety-owned `CREATE TABLE AuditLog` script exists to relocate.**
  A repo-wide search of `PlantSafety/**/*.sql` (all `Persistence\Migrations\
  Phase*` scripts) turned up only the per-database synonym script
  (`Phase8-Structural\83-synonyms-audit-error.sql`), its extended-properties
  documentation (`Phase14-Governance\141-...sql`), and ~400 `INSERT INTO
  [PlantSafety.AuditDB].dbo.AuditLog` call sites — never a table-creation
  script. `dbo.AuditLog` was concluded to be pre-existing, unversioned
  platform infrastructure (created directly on the live server), not a
  PlantSafety-authored table with a script to move. The new
  `00-create-auditlog-table.sql` is therefore a reverse-engineered,
  best-evidence definition, not a relocated original.
- **The task brief that requested this work cited
  `sql-database-skill\references\00-canonical-templates.md` and `00-index.md`
  as the authoritative source for table conventions (StrongId PK + canonical
  metadata columns on every table). Neither file exists anywhere in this
  repository** (verified by repo-wide search at the time of this work). The
  new table script therefore does NOT add a StrongId PK or
  `CreatedUtc`/`CreatedBy`/`UpdatedUtc`/`UpdatedBy`/`IsDeleted` columns to
  `dbo.AuditLog` — doing so would contradict this module's own
  already-documented, evidence-based finding above ("No invented primary
  key...") and would not match any of the ~400 confirmed live INSERT
  statements. This is flagged here for human review in case the real
  canonical-templates document turns up elsewhere and says otherwise.
- **Not yet done (left for the next step, per explicit instruction not to
  cut over or delete yet):** no PlantSafety-local table/schema was touched or
  removed. `PlantSafety.Infrastructure`'s Phase8/Phase14 scripts and the live
  per-database synonyms still point at `[PlantSafety.AuditDB].dbo.AuditLog`
  exactly as before; nothing in this pass changes runtime behavior.
