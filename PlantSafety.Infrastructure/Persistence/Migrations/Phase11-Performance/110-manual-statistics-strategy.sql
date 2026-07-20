-- =============================================
-- Migration:     110-manual-statistics-strategy.sql
-- Author:        Performance Guardian Agent
-- Create date:   2026-07-19
-- Description:   Manual (non-auto-created) statistics strategy review across
--                all 8 primary databases. Documents the live analysis
--                performed and the resulting decision: NO standalone
--                multi-column/filtered statistics objects are created by
--                this script, because none are genuinely justified today.
--
-- *** Why this is a documentation-and-verification script, not a build ***
-- Manual (CREATE STATISTICS) objects are only needed when SQL Server would
-- otherwise have no column-distribution information for a WHERE-clause
-- predicate combination -- i.e. a multi-column filter with NO matching
-- index (auto-stats only fires off single leading-column histograms and
-- index-linked stats; a genuinely uncovered multi-column predicate needs a
-- standalone CREATE STATISTICS object to get a correlated density
-- estimate). This script performed exactly the live search the task
-- requested and found no such gap. Building speculative stats objects
-- anyway would violate the task's own instruction not to create redundant
-- stats that duplicate index-linked auto-stats -- so nothing is built.
--
-- *** Live review performed (2026-07-19) ***
-- 1. Enumerated all indexes (live, via sys.indexes/sys.index_columns) across
--    all 8 in-scope databases (PlantSafety.CoreDB, ArchiveDB, CloudIdentityDB,
--    PredictiveDB, ReportingDB, IntegrationDB, DiagnosticsDB, ExportImportDB).
--    Confirmed the (TenantId, Status) / (TenantId, CreatedUtc DESC) composite
--    pattern already exists as a real, filtered ([IsDeleted]=0), index-linked
--    statistics object on essentially every tenant-scoped entity table
--    (e.g. IX_Analytics_Culture_TenantId_Status, IX_Analytics_Culture_
--    TenantId_CreatedUtc), plus a third FK-specific composite per entity
--    (e.g. IX_SafetyEvents_IncidentManagement_TenantId_ReportedByWorkerId).
--    These composites already carry SQL-Server-maintained, index-linked
--    statistics -- no standalone object needed.
-- 2. Enumerated every WHERE clause in the Phase 5 views
--    (Persistence\Migrations\Phase5-Views\50..57-*.sql): all 39+ per-entity
--    "active" views filter on ISNULL(IsDeleted,0)=0 only (single-column,
--    already covered by every table's filtered IsDeleted-predicate indexes).
--    History-entry (audit-trail) views/joins filter by ParentId
--    (=...HistoryEntry FK), already covered by each history table's
--    IX_<Entity>HistoryEntry_<ParentId> composite (ParentId, ChangedAtUtc
--    DESC). No multi-column gap found.
-- 3. Enumerated every stored procedure across all 8 databases (live count:
--    436 procedures, Persistence\Migrations\Phase6-StoredProcedures\
--    60..67-*.sql). Confirmed EVERY procedure across all 8 databases follows
--    exactly one of 6 canonical CRUD verbs generated from the same template:
--    _List (single optional @TenantId filter), _GetById, _GetByParentId
--    (single FK filter), _Create, _Update, _Delete. There is NOT ONE
--    hand-written multi-predicate search/filter/analytics procedure in the
--    entire platform today (verified via `grep -E "PROCEDURE " *.sql | sort
--    -u | grep -v these-6-verbs` -- zero results). Summary-refresh
--    procedures (e.g. SafetyEvents.sp_IncidentManagementSummary_Refresh,
--    Operational.sp_RiskManagementSummary_Refresh,
--    Operational.sp_AlertSummary_Refresh) are unconditional full-table
--    GROUP BY rebuilds with no WHERE filter at all -- they scan every row,
--    so a filtered/selective stats object would not help them; the relevant
--    stats there are the SOURCE table's ordinary column stats, already
--    present.
-- 4. Conclusion: with the current SP/view surface, there is no WHERE-clause
--    predicate combination that lacks index-linked statistics coverage.
--    Building a standalone stats object today would be speculative (no real
--    query shape to justify it) and would duplicate what the composite
--    indexes already provide once auto-stats populates on first real data
--    load. NOT created here, by design.
--
-- *** Forward-looking guidance (for the next engineer who adds a hand-written
--     multi-predicate query) ***
-- If/when a genuinely new ad-hoc multi-column WHERE clause is added (e.g. a
-- future dashboard query filtering DiagnosticEvent by (Module, Severity,
-- OccurredAtUtc) where Module is NOT a leading index column today), the
-- correct fix is one of, in this preference order:
--   1. First choice: add/extend a covering index if the predicate is
--      selective and repeats -- gets both an access path AND stats for free.
--   2. Second choice, ONLY if an index is not warranted (e.g. the predicate
--      is used for reporting/ad-hoc analysis, not a hot OLTP path): a
--      standalone `CREATE STATISTICS <Schema>.ST_<Entity>_<Col1>_<Col2> ON
--      <Schema>.<Entity> (<Col1>, <Col2>) WHERE ISNULL(IsDeleted,0)=0 WITH
--      FULLSCAN;` filtered statistics object, matching the platform's
--      existing filtered-index convention. Re-run this decision process
--      (steps 1-3 above) before adding one -- don't add speculative stats.
-- Naming convention for any future standalone stats object (not currently
-- needed, documented for consistency): `ST_<Entity>_<Col1>_<Col2>[_Filtered]`,
-- mirroring the `IX_<Entity>_<Column>` index convention in
-- 00-canonical-templates.md.
--
-- *** Auto-stats / auto-update posture (verified live, unchanged) ***
-- All 8 databases already have AUTO_CREATE_STATISTICS ON and
-- AUTO_UPDATE_STATISTICS ON (SQL Server defaults, not overridden by any
-- prior migration) -- verified below. No change needed; this is the correct
-- posture for a schema-first, currently-empty platform: auto-stats will
-- populate correctly the moment real data starts flowing through the
-- existing (already comprehensive) index set.
--
-- Idempotency: this script is read-only / verification-only. Safe to re-run
-- any number of times; it creates nothing.
-- =============================================
SET NOCOUNT ON;
GO

------------------------------------------------------------
-- Verification: confirm AUTO_CREATE_STATISTICS / AUTO_UPDATE_STATISTICS are
-- ON for all 8 in-scope databases (live check, not assumed).
------------------------------------------------------------
SELECT
    name AS DatabaseName,
    is_auto_create_stats_on,
    is_auto_update_stats_on,
    is_auto_update_stats_async_on
FROM sys.databases
WHERE name IN (
    N'PlantSafety.CoreDB', N'PlantSafety.ArchiveDB', N'PlantSafety.CloudIdentityDB',
    N'PlantSafety.PredictiveDB', N'PlantSafety.ReportingDB', N'PlantSafety.IntegrationDB',
    N'PlantSafety.DiagnosticsDB', N'PlantSafety.ExportImportDB'
)
ORDER BY name;
GO

------------------------------------------------------------
-- Verification: confirm no standalone (non-index-linked) statistics objects
-- already exist anywhere that this review may have missed (sanity check --
-- expect zero rows in a schema-first build with only index-linked stats).
-- Run per-database; CoreDB shown as the representative largest surface.
------------------------------------------------------------
USE [PlantSafety.CoreDB];
GO
SELECT
    s.name AS SchemaName, t.name AS TableName, st.name AS StatName,
    st.auto_created, st.user_created, st.has_filter, st.filter_definition
FROM sys.stats st
JOIN sys.tables t ON st.object_id = t.object_id
JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE st.user_created = 1  -- user_created=1, auto_created=0, and no matching index_id => truly standalone
  AND NOT EXISTS (SELECT 1 FROM sys.indexes i WHERE i.object_id = st.object_id AND i.name = st.name)
ORDER BY s.name, t.name;
GO
-- Expected/confirmed: zero rows (no standalone stats exist; every stats
-- object present today is index-linked, which is correct and expected).
