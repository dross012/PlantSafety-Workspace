# Phase 5 Schema Changes

No dedicated schema-diff tool exists in this codebase (checked `SafetyTooling\SchemaSync` — it is a
build-time SQL-object generator/analyzer for existing schemas, not a diff/compare tool against a
prior baseline; no other schema-diff utility was found). Building one is out of realistic scope for
this documentation pass, per this task's instructions. This doc instead lists, by direct inspection
of the `.sql` files under `PlantSafety.Infrastructure`, the concrete new schema surfaces Phase 5
added. All scripts share the same header convention (`Project` / `Module` / `Layer` / `SchemaVersion:
2026.1` / `Additive-only evolution`) and target `USE [PlantSafety.CoreDB];`.

## `[Workflow]` schema — new

Module: `PlantSafety.Infrastructure\Workflow\Sql`

Tables (`Sql\Tables`):
- `[Workflow].[WorkflowDefinition]`
- `[Workflow].[WorkflowStepDefinition]`
- `[Workflow].[WorkflowTransition]`
- `[Workflow].[WorkflowInstance]` (`WorkflowInstance.sql`)
- Follow-on script: `030-workflow-graph-foreign-keys.sql`

Stored procedures (`Sql\Procedures`):
- `usp_WorkflowDefinition_GetAllActive`, `usp_WorkflowDefinition_GetById`,
  `usp_WorkflowDefinition_Insert`, `usp_WorkflowDefinition_Update`
- `usp_WorkflowStepDefinition_GetByDefinitionId`, `usp_WorkflowStepDefinition_Upsert`
- `usp_WorkflowTransition_GetByDefinitionId`, `usp_WorkflowTransition_Upsert`
- `usp_WorkflowInstance_GetById`, `usp_WorkflowInstance_GetPendingApprovalsForActor`,
  `usp_WorkflowInstance_Insert`, `usp_WorkflowInstance_List`, `usp_WorkflowInstance_Update`
- `usp_WorkflowStepInstance_GetByInstanceId`, `usp_WorkflowStepInstance_Upsert`

Tenant-scoped (NOT NULL `TenantId uniqueidentifier`) so the platform's Row-Level-Security discovery
picks the tables up automatically (per `WorkflowDefinition.sql` header).

## `[AnalyticsUnifiedModel]` schema — new

Module: `PlantSafety.Infrastructure\Analytics\UnifiedModel\Sql`

Dimension tables: `DimAsset`, `DimDate`, `DimEmployee`, `DimShift`, `DimSite`.

Fact tables: `FactAuditFinding`, `FactContinuousImprovementAction`, `FactMaintenanceWorkOrder`,
`FactOperationalEvent`, `FactRiskAssessment`, `FactSchedulingEvent`.

Rollup: `DailyAnalyticsSnapshot` (append-only, immutable historical snapshot — "Rows are never
updated or deleted" per header comment).

Follow-on scripts (applied after base tables): `030-dashboard-covering-indexes.sql`,
`040-fact-dimension-foreign-keys.sql`, `050-fact-table-partitioning.sql`.

Dashboard-read stored procedures: `usp_AuditDashboard_GetDaily`,
`usp_ContinuousImprovementDashboard_GetDaily`, `usp_MaintenanceDashboard_GetDaily`,
`usp_ProductionDashboard_GetDaily`, `usp_RiskDashboard_GetDaily`, `usp_SchedulingDashboard_GetDaily`
— one per unified-model domain, all following the same `_GetDaily` naming convention.

## `[ComplianceRulesEngine]` schema — new, deliberately separate from `[Compliance]`

Module: `PlantSafety.Infrastructure\Compliance\RulesEngine\Sql`

The header comment on `ComplianceEvaluation.sql` explicitly documents the design decision: this is
a **new, dedicated schema**, distinct from the pre-existing `[Compliance]` schema (which backs
application/software-audit read-models such as `usp_AuditFindings_GetFindings`,
`usp_CertificationAggregate_*`), specifically so the two never collide on table/procedure names.

Tables:
- `[ComplianceRulesEngine].[ComplianceEvaluation]`
- `[ComplianceRulesEngine].[ComplianceRuleResult]` (child entity; `ViolationsJson` column stores
  rule violations as a JSON array rather than a further child table, per header comment)
- Follow-on script: `020-additional-indexes.sql`

Stored procedures: `usp_ComplianceEvaluation_GetAllForTenant`, `usp_ComplianceEvaluation_GetById`,
`usp_ComplianceEvaluation_GetHistoryForTarget`, `usp_ComplianceEvaluation_Insert`,
`usp_ComplianceRuleResult_GetByEvaluationId`, `usp_ComplianceRuleResult_Insert`.

## `[Auditing]` schema — additions

Two sub-areas were added under the existing `[Auditing]` schema:

### `Auditing\AuditReport` (Handbook Phase 5 A6.2 printable audit reports)

Tables: `[Auditing].[AuditReport]`, `[Auditing].[AuditReportSection]` (child entity).
Per the header comment, the rendered report byte-stream is **not** stored in this table — it is
persisted to blob storage via `IAuditReportContentStore` / `AuditReportBlobContentStore` and only
referenced from the row via `RenderedContentReference`.

Stored procedures: `usp_AuditReport_GetById`, `usp_AuditReport_GetForTarget`,
`usp_AuditReport_InsertDraft`, `usp_AuditReport_UpdateOutcome`,
`usp_AuditReportSection_GetByReportId`, `usp_AuditReportSection_Insert`.

### `Auditing\AuditTimeline`

Table: `[Auditing].[ConcurrencyConflictRecord]`, plus follow-on `020-covering-index.sql`.

Stored procedures: `usp_ConcurrencyConflictRecord_GetById`,
`usp_ConcurrencyConflictRecord_GetForEntity`, `usp_ConcurrencyConflictRecord_Insert`,
`usp_OutboxMessages_GetByTenant` (reads the platform's shared `System.OutboxMessages` table for
timeline display rather than owning a new outbox table).

## Pre-existing schemas Phase 5 did **not** change

`PlantSafety.Infrastructure\Auditing\Sql` (the older `AuditEngagement` / `AuditFinding` /
`AuditCategoryRecord` tables, `Created: 2026-07-21`) and
`PlantSafety.Infrastructure\Compliance\Sql` (the older `AuditingAggregate` /
`CertificationAggregate` / `CAPATriggers` procedures) both also carry `SchemaVersion: 2026.1`
headers and are correctly tenant-scoped, so schema version alone does not distinguish "Phase 5" work
from immediately-prior work here. The concrete, verifiable distinction is the `Created:` date on
each file's header: the new schema/table/procedure files listed above (`Workflow`,
`AnalyticsUnifiedModel`, `ComplianceRulesEngine`, `Auditing\AuditReport`, `Auditing\AuditTimeline`)
are all dated `2026-08-02` (today, per this session), while `Auditing\Sql\Tables\AuditEngagement.sql`
is dated `2026-07-21` and the various `Compliance\Sql\Procedures\*` files are undated in their
header's `Created:` field or predate the Phase 5 pass. These older `[Auditing]`/`[Compliance]`
surfaces were left untouched by Phase 5 and continue to serve the pre-existing
application/software-audit read-model use case, which `AuditEngagement.sql`'s own header explicitly
calls out as the reason `[Auditing]` and `[Compliance]` are kept as two distinct schemas.
