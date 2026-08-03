# Query-Plan-Hint Convention (Phase 6)

## Status

**New as of Phase 6 (2026-08-02).** No `OPTION (RECOMPILE)` / `OPTIMIZE FOR` convention existed
anywhere in this codebase before this pass (confirmed by a repo-wide scan of every `*.sql` file
under `PlantSafety.Infrastructure\**\Sql\Procedures\` — zero hits for `RECOMPILE` or
`OPTIMIZE FOR`). This document is the first statement of the convention and applies platform-wide,
not just to the `AnalyticsUnifiedModel` schema; it lives here because the six dashboard procedures
below are the motivating, highest-value case.

## The problem this addresses

Several stored procedures in this codebase take an **optional filter parameter** with the shape:

```sql
@SiteId UNIQUEIDENTIFIER = NULL
...
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
```

SQL Server compiles and caches **one** plan per procedure, built from whichever parameter values
happen to be supplied on the first call after a (re)compile ("parameter sniffing"). Because
`@SiteId IS NULL` (all sites, wide result set) and `@SiteId = <value>` (one site, narrow result
set) are dramatically different in selectivity, a plan compiled for one shape (e.g. cached row-count
estimates, memory grants, and — where the optional column is part of an index key — even the
seek/scan strategy itself) can be badly wrong for the other shape. This is a well-known,
plausible risk anywhere this pattern appears, not just where it has been directly observed
misbehaving.

## Decision table

| Hint | When to use | When NOT to use |
|---|---|---|
| **`OPTION (RECOMPILE)`** | Low/moderate call frequency (human-triggered, e.g. dashboard refresh, report generation) **and** the query does cardinality-sensitive downstream work (`GROUP BY`/aggregates, sort, memory-grant-sized operators) where a bad row-count estimate causes a bad memory grant or spill — not just a different index choice. Pay the per-call recompile cost to get an exact-cardinality plan every time. | High-frequency call sites (per-request list/grid endpoints, hot OLTP paths) — the repeated compilation CPU cost outweighs the benefit. |
| **`OPTIMIZE FOR UNKNOWN`** | Higher-frequency call sites (list/grid read endpoints) where the optional column may or may not be a leading/secondary key of the index being seeked, so the *risk* is a skewed plan cached from an unlucky first caller, but the query itself is simple enough (no aggregation, index already covers the ORDER BY) that an "average selectivity" density-vector-based estimate is good enough. Zero per-call recompile cost — the plan is compiled once using a density-vector estimate instead of the sniffed literal. | Queries where getting cardinality right materially changes the plan shape (join order, spool vs. no spool) on every call — there `RECOMPILE` is worth the cost. |
| **Neither** | The optional predicate does not change plan shape at all — e.g. it's a secondary filter applied after a clustered/unique-key point lookup that already returns at most one row (optimistic-concurrency `@ETag IS NULL OR ...` guards on single-row `UPDATE ... WHERE Id = @Id AND (@ETag IS NULL OR ...)` are the canonical example), so there is nothing for a bad cardinality estimate to distort. | N/A |

## Applied (Phase 6)

### `OPTION (RECOMPILE)` — the six `[AnalyticsUnifiedModel].usp_*Dashboard_GetDaily` procedures

`usp_SchedulingDashboard_GetDaily`, `usp_ProductionDashboard_GetDaily`,
`usp_MaintenanceDashboard_GetDaily`, `usp_RiskDashboard_GetDaily`,
`usp_ContinuousImprovementDashboard_GetDaily`, `usp_AuditDashboard_GetDaily`
(`PlantSafety.Infrastructure\Analytics\UnifiedModel\Sql\Procedures\`).

Rationale: BI-dashboard, human-triggered call frequency (not a hot per-request path); every one
of the six does a `GROUP BY <date>` with `SUM`/`AVG`/`COUNT` aggregates, so the memory grant and
aggregate-hash sizing are cardinality-sensitive. The Phase 5 hardening pass already added a
covering `(TenantId, <EventTimestampColumn>) INCLUDE (SiteKey, ...)` index per fact table
(`030-dashboard-covering-indexes.sql`) — `SiteKey`/`SiteId` is only an `INCLUDE`d column, not a key
column, so `RECOMPILE` does not change *which* index is used (it was already a stable
`TenantId`-prefix seek regardless of `@SiteId`); it only fixes the row-count estimate feeding the
aggregate/memory-grant operators for the actual `@SiteId` value on each call.

### `OPTIMIZE FOR UNKNOWN` — ten `[Auditing]` list/read procedures with the `@SiteId IS NULL OR` pattern

`usp_AuditEngagement_ListByTenant`, `usp_AuditFinding_ListByTenant`, `usp_Efficiency_GetItems`,
`usp_FacilityAssessment_GetItems`, `usp_Inspecting_GetItems`, `usp_Operations_GetItems`,
`usp_Procedures_GetItems`, `usp_QualityControl_GetItems`, `usp_RegulatoryCompliance_GetItems`,
`usp_Systems_GetItems` (`PlantSafety.Infrastructure\Auditing\Sql\Procedures\`).

Rationale: these back list/grid read endpoints (`IAuditEngagementRepository.ListAsync` and
siblings), a higher call-frequency shape than a BI dashboard, so `RECOMPILE`'s per-call CPU cost is
avoided. `IX_AuditEngagement_TenantId_SiteId` genuinely has `SiteId` as a **key** column (not just
`INCLUDE`d), so an all-sites call (`@SiteId IS NULL`, prefix seek on `TenantId` alone) and a
single-site call (`@SiteId = <value>`, full two-column seek) are two different seek shapes — the
clearest case in this codebase of the "unlucky first caller" risk `OPTIMIZE FOR UNKNOWN` protects
against. The other nine procedures return simple, non-aggregated row sets (no `GROUP BY`) where an
average-selectivity density-vector estimate is good enough; applying the same hint to all ten
keeps the convention consistent per-table/per-module rather than picking case-by-case.

### Neither — `[Workflow].usp_WorkflowDefinition_Update` / `usp_WorkflowInstance_Update`

Both contain a superficially similar `(@ETag IS NULL OR CONVERT(...) = @ETag)` guard, but it is
applied on top of an `UPDATE ... WHERE <ClusteredKey> = @Id AND TenantId = @TenantId AND
IsDeleted = 0 AND (...)` — a point lookup on the clustered primary key that already narrows to at
most one row before the `@ETag` predicate is even evaluated. There is no cardinality-sensitive
plan shape here for a hint to stabilize, so per the decision table these two procedures are
deliberately left unhinted.

### Not applicable — `[ComplianceRulesEngine]` procedures

Scanned `PlantSafety.Infrastructure\Compliance\RulesEngine\Sql\Procedures\` for the
`@X IS NULL OR` pattern: none of the seven procedures there (`usp_ComplianceEvaluation_GetAllForTenant`,
`usp_ComplianceEvaluation_GetById`, `usp_ComplianceEvaluation_GetHistoryForTarget`,
`usp_ComplianceEvaluation_Insert`, `usp_ComplianceRuleResult_BulkInsert`,
`usp_ComplianceRuleResult_GetByEvaluationId`, `usp_ComplianceRuleResult_Insert`) take any optional
filter parameter — every parameter is mandatory, so there is no parameter-sniffing-prone shape to
hint.

## Verifying the covering-index benefit is preserved

Both hints only change how SQL Server *estimates cardinality*; neither hint disables or bypasses
an index. `OPTION (RECOMPILE)` still uses the normal cost-based optimizer (with exact, sniffed
literal values) to choose indexes; `OPTIMIZE FOR UNKNOWN` still uses the normal cost-based
optimizer, just seeded with the column's density vector instead of a literal. In every procedure
hinted above, the `(TenantId, ...)` or `(TenantId, SiteId)` leading key columns of the relevant
covering index are still supplied as equality/range predicates directly from `@TenantId` (never
`UNKNOWN`), so the seek on the covering index added by
`030-dashboard-covering-indexes.sql`/`AuditEngagement.sql`/`AuditFinding.sql`/
`AuditCategoryRecord.sql` is unaffected by either hint.
