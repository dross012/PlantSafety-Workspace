# Shared.SafetyData

## What this module is

`Shared.SafetyData` is the canonical **read path** for PlantSafety's real
Hazard/Risk, Corrective Action, and derived Safety Compliance data, backing
three Reporting report types:

- **Hazard Summary** — `[PlantSafety.CoreDB].[RiskMgmt].[Risk]` joined to
  `[RiskMgmt].[ControlMeasure]`.
- **Corrective Action** — `[PlantSafety.CoreDB].[ContinuousImprovement].[CAPA]`.
- **Safety Compliance** — a *derived* open/closed/overdue rollup computed from
  the same two tables above (no dedicated PlantSafety compliance
  aggregate/table exists — see `SafetyComplianceEntry.cs` and
  `sql/Phase1-StoredProcedures/03-usp-get-safety-compliance.sql` for the full
  derivation).

This module exists so Reporting can query real PlantSafety reporting data
without taking a forbidden direct project dependency on PlantSafety (module
boundary rule) — the exact pattern `Shared.AuditLogging` already established
for `dbo.AuditLog`. See `Shared/AuditLogging/docs/README.md` for that
precedent.

## Why it exists (history)

`Reporting.Infrastructure`'s `SafetyDataSourceProvider`
(`Reporting/Reporting.Infrastructure/DataSources/Safety/
SafetyDataSourceProvider.cs`) shipped 2026-07-27 as a stub returning
`HazardSummaryData.Empty` / `CorrectiveActionReportData.Empty` /
`SafetyIncidentData.Empty`, with an explicit TODO flagging that
Reporting must not depend on PlantSafety directly and that a shared module
(mirroring `Shared/AuditLogging/`) exposing read repositories both modules can
reference was the recommended fix. `Shared.SafetyData` is that module.

**Scope note:** this pass builds `Shared.SafetyData` itself only. Wiring
`PlantSafety` and `Reporting` to actually consume it (registering
`AddSharedSafetyData` in each API's DI, updating
`SafetyDataSourceProvider`/`ISafetyDataSourceProvider` to call
`ISafetyDataRepository`, deriving a `SafetyComplianceData`
`Reporting.Domain.ValueObjects` type and the report type/generator/PDF
template for Safety Compliance) is an explicitly out-of-scope follow-up step.

## Layout

- **`Shared.SafetyData.Abstractions`** (net8.0, no external package
  dependencies) — `HazardSummaryEntry`, `CorrectiveActionEntry`,
  `SafetyComplianceEntry`, `SafetySearchCriteria`, `SafetySearchResult<TEntry>`,
  `ISafetyDataRepository`. `HazardSummaryEntry` and `CorrectiveActionEntry` are
  field-for-field identical to `Reporting.Domain.ValueObjects.
  HazardSummaryEntry` / `CorrectiveActionEntry` (Reporting Module Integration
  pass, 2026-07-27), so mapping in a future `SafetyDataSourceProvider` rewrite
  is a direct 1:1 projection.
- **`Shared.SafetyData.Infrastructure`** (net8.0) —
  `SafetyDataDapperRepository` (Dapper, calls `dbo.usp_GetHazardSummary` /
  `dbo.usp_GetCorrectiveActions` / `dbo.usp_GetSafetyCompliance`),
  `SafetyDataOptions` (bound from configuration section `Shared:SafetyData`,
  targeting `PlantSafety.CoreDB`), `SafetyDataQueryException`, and
  `ServiceCollectionExtensions.AddSharedSafetyData(...)` for DI registration.
- **`Shared.SafetyData.Tests`** — unit tests for the Abstractions record
  shapes/defaults and the `AddSharedSafetyData` DI extension, mirroring the
  coverage `Shared.AuditLogging.Tests` has for its own module.
- **`sql/Phase1-StoredProcedures/01-usp-get-hazard-summary.sql`** — creates
  `dbo.usp_GetHazardSummary` against `[PlantSafety.CoreDB]`. Returns **three**
  result sets (count, page of `Risk` rows, then all `ControlMeasure` rows for
  those `Risk` rows) — see that file's header comment for why control measures
  are a separate result set rather than a joined/duplicated row set.
- **`sql/Phase1-StoredProcedures/02-usp-get-corrective-actions.sql`** —
  creates `dbo.usp_GetCorrectiveActions` against `[PlantSafety.CoreDB]`.
  Returns two result sets (count, then page), mirroring
  `dbo.usp_SearchAuditLog`'s shape.
- **`sql/Phase1-StoredProcedures/03-usp-get-safety-compliance.sql`** —
  creates `dbo.usp_GetSafetyCompliance` against `[PlantSafety.CoreDB]`, a
  derived, non-paginated rollup. See that file's extensive header comment for
  the open/closed/overdue classification rules and the judgment calls flagged
  for human review.

Unlike `Shared.AuditLogging` (which has table-definition scripts under
`sql/Phase0-Tables/` because `dbo.AuditLog` had no tracked `CREATE TABLE`
script anywhere in the repo), this module does **not** add its own
`Phase0-Tables` scripts: `[RiskMgmt].[Risk]`, `[RiskMgmt].[ControlMeasure]`,
and `[ContinuousImprovement].[CAPA]` all already have real, tracked,
PlantSafety-owned `CREATE TABLE` scripts (see the SQL scripts' header comments
for exact paths) — this module only adds new stored procedures on top of
those existing, already-versioned tables.

## Consumers

None yet — this is a from-scratch module build. The intended, not-yet-wired
consumer is:

- **Reporting** (`Reporting.Infrastructure`) — a future revision of
  `SafetyDataSourceProvider` would inject
  `Shared.SafetyData.Abstractions.ISafetyDataRepository`, call
  `GetHazardSummaryAsync`/`GetCorrectiveActionsAsync`/
  `GetSafetyComplianceAsync` in real time (no caching, no ETL — matching
  `AuditDataSourceProvider`'s pattern), and map `HazardSummaryEntry`/
  `CorrectiveActionEntry` directly onto the identically-shaped
  `Reporting.Domain.ValueObjects` records (a new `SafetyComplianceData`
  value object would need to be added to `Reporting.Domain.ValueObjects` and
  wired into `IReportDataSource`/`IReportDataSource` for the Safety
  Compliance rollup, since no such report type exists yet).

`PlantSafety` itself is not expected to consume this module — PlantSafety
already owns and mutates these tables directly through its own aggregates
(`Risk`/`RiskDapperRepository`, CAPA workflow). `Shared.SafetyData` exists
purely to give *other* modules (starting with Reporting) a safe read-only path
that does not require a direct PlantSafety project reference.

## Safety invariants

- **Read-only.** Nothing in this module may INSERT, UPDATE, or DELETE against
  `RiskMgmt.Risk`, `RiskMgmt.ControlMeasure`, or `ContinuousImprovement.CAPA`.
  All three stored procedures are SELECT-only. This must never change — these
  tables are owned and mutated exclusively by PlantSafety's own
  aggregates/repositories.
- **Tenant-scoped, not server-level.** Unlike `Shared.AuditLogging`'s
  `dbo.AuditLog` (server-level, cross-database), every table this module reads
  is `TenantId`-scoped. `SafetySearchCriteria.TenantId` is required (not
  optional) on every call.
- **No invented columns.** Every column name in every SQL script and every
  Abstractions model is copied verbatim from a real, tracked PlantSafety SQL
  script or a confirmed C# read-model shape — see each file's header comment
  for the exact evidence trail. Where no clean 1:1 mapping exists (Safety
  Compliance), the derivation is explicitly documented rather than invented
  silently.
- **SQL injection discipline.** Every filter value is bound as a stored
  procedure parameter; nothing is ever concatenated into SQL text.
- **No EF Core.** Dapper + stored procedure only, matching every other module
  in this solution.

## Judgment calls made during this build (2026-07-27)

- **Corrective Action table choice**: PlantSafety.Infrastructure has *two*
  separate corrective-action concepts — `[ContinuousImprovement].[CAPA]`
  (real, tracked `CREATE TABLE`/`CREATE PROCEDURE` scripts) and an implied
  `[ContinuousImprovement].[CorrectiveAction]` table (referenced only by
  `CorrectiveActionDapperRepository`'s C#, with no tracked SQL scripts
  anywhere in the repo). This module targets `CAPA`, the verified schema, per
  the "never invent column names" rule. See `CorrectiveActionEntry.cs`'s
  header comment for the full trail, flagged for a future human/DBA to
  confirm the live `CorrectiveAction` table's real schema in case Reporting
  later needs that richer shape (`SourceEventId`/`SourceEventType`/
  `AssignedToWorkerId`/`Priority`/`CompletedAtUtc`/`ETag`) instead.
- **`HazardSummaryEntry.Category` vs `RiskLevel` naming**: `RiskMgmt.Risk`'s
  own `Category` column actually stores the qualitative `RiskLevel` Smart Enum
  code (Low/Medium/High/Critical) per `Risk.cs`'s own XML doc, not a hazard
  category — renamed to `RiskLevel` on this module's model to avoid confusion,
  with `HazardSource` mapped to this model's `Category` field instead (the
  closest real "what kind of hazard is this" free-text field on the table).
- **Safety Compliance derivation**: no PlantSafety aggregate/table maps
  cleanly to "Safety Compliance". Built as a two-row (`Hazards` /
  `CorrectiveActions`) open/closed/overdue rollup over the same two verified
  tables the other two procedures read from, with the open/closed/overdue
  predicates explicitly documented and flagged for review in
  `03-usp-get-safety-compliance.sql`'s header comment, per the task's explicit
  instruction to derive and document rather than invent a fictitious
  "SafetyCompliance" table.
- **Generic `SafetySearchResult<TEntry>`** instead of one non-generic result
  type per entry (`HazardSummarySearchResult`/`CorrectiveActionSearchResult`):
  both paginated queries share an identical count-then-page shape, so a single
  generic envelope avoids duplicating it. `GetSafetyComplianceAsync` returns
  `IReadOnlyList<SafetyComplianceEntry>` directly instead, since it is a small,
  non-paginated rollup, not a page of search results.
- **DI extension naming**: `AddSharedSafetyData(IServiceCollection,
  IConfiguration)`, mirroring `Shared.AuditLogging`'s
  `AddSharedAuditLogging(...)` and the single `Add<Module>` extension pattern
  used elsewhere in the solution.
- **Not wired to any consumer yet** (explicit instruction): `PlantSafety` and
  `Reporting` are unmodified by this pass. No project reference to
  `Shared.SafetyData` exists from either module yet.
