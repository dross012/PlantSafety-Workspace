-- =============================================
-- Migration:     112-columnstore-diagnosticeventfact.sql
-- Author:        Performance Guardian Agent
-- Create date:   2026-07-19
-- Description:   Adds a NONCLUSTERED columnstore index to
--                Diagnostics.DiagnosticEventFact -- the one genuine
--                analytical star-schema fact table found in this platform's
--                current index re-review.
--
-- *** Candidate review performed (2026-07-19) -- why only this table ***
-- Reviewed every "Fact"/"Summary"/"Dimension"-named table across all 8
-- primary databases via live sys.columns inspection:
--   - Diagnostics.DiagnosticEventFact (THIS migration): a genuine star-
--     schema fact table -- joins to Diagnostics.DiagnosticEventDimension via
--     DiagnosticEventDimensionId, carries two measure columns (EventCount,
--     DurationMs) and grouping/dimension columns (TenantId, DateKey,
--     Severity). This is exactly the shape columnstore is designed for:
--     wide analytical scans/aggregations (SUM(EventCount), AVG(DurationMs)
--     GROUP BY TenantId, DateKey, Severity), not singleton row lookups.
--   - Diagnostics.DiagnosticDailySummary, Operational.AlertSummary,
--     Operational.RiskManagementSummary, SafetyEvents.
--     IncidentManagementSummary, Integration.IntegrationLogDailySummary,
--     Integration.ModuleHealthDailySummary, ExportImport.
--     ExportOperationDailySummary, ExportImport.ImportOperationDailySummary:
--     all are PRE-AGGREGATED summary tables with a unique composite key
--     (SummaryDateKey + grouping columns) queried by point lookup on that
--     key, not scanned/aggregated further -- rowstore B-tree indexes are the
--     correct structure for these; columnstore would add compression/
--     maintenance overhead for zero analytical-scan benefit. NOT converted.
--   - Diagnostics.DiagnosticEventDimension: the dimension side of the star
--     schema -- small, narrow, point-looked-up by key from the fact table,
--     not itself scanned/aggregated. Correctly stays rowstore.
--   - No OLTP CRUD entity table (the ~60 per-module Analytics/Compliance/
--     Operational/SafetyEvents tables) was considered -- these are
--     transactional read/write tables accessed by TenantId/Status/CreatedUtc
--     point/range lookups from the generated CRUD procedure surface (see
--     110-manual-statistics-strategy.sql), the opposite access pattern from
--     what columnstore optimizes for. Correctly NOT touched.
--
-- *** Why NONCLUSTERED, not CLUSTERED columnstore ***
-- A clustered columnstore index replaces the table's storage entirely and
-- would remove the existing rowstore clustered PK
-- (PK_Diagnostics_DiagnosticEventFact on DiagnosticEventFactId) and its
-- point-lookup/OLTP access pattern (the fact table is still written
-- row-at-a-time by the ETL/aggregation path that populates it from
-- DiagnosticEvent). A NONCLUSTERED columnstore index preserves the existing
-- rowstore clustered PK and all existing nonclustered B-tree indexes for
-- OLTP access, while adding a parallel column-oriented copy of the
-- analytically-relevant columns for batch-mode aggregate query performance
-- -- exactly the task's explicit guidance ("nonclustered, not clustered, to
-- preserve existing OLTP access patterns").
--
-- *** Column selection ***
-- Includes every column genuinely useful for analytical grouping/filtering/
-- aggregation: DiagnosticEventId (drill-back to source event),
-- DiagnosticEventDimensionId (join key to the dimension table), TenantId,
-- DateKey, Severity (grouping/filtering columns), DurationMs, EventCount
-- (measures), CreatedUtc (time-series filtering). Excludes
-- DiagnosticEventFactId (surrogate PK, not used in any analytical
-- GROUP BY/aggregate -- already available via the existing rowstore
-- clustered PK for the rare row-level lookup) and CreatedBy/IsDeleted
-- (governance/audit columns, not analytically meaningful measures or
-- dimensions).
--
-- *** 0-row caveat (explicitly flagged) ***
-- Diagnostics.DiagnosticEventFact currently has 0 rows (schema-first
-- build-out). A columnstore index provides zero query-time benefit until
-- real row volume exists (in fact, SQL Server keeps very small columnstore
-- segments in a rowstore "deltastore" until ~1,048,576-row rowgroups can be
-- formed) -- this migration builds the STRUCTURE now (correct architectural
-- placement, zero cost to build against an empty table) so it is already in
-- place and maintained by every future INSERT once the Diagnostics module's
-- fact-load path goes live, rather than requiring an online index build
-- against a populated production table later.
--
-- Idempotency: existence-guarded; safe to re-run.
-- =============================================
SET NOCOUNT ON;
GO
USE [PlantSafety.DiagnosticsDB];
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID('Diagnostics.DiagnosticEventFact')
      AND name = 'CSI_Diagnostics_DiagnosticEventFact_Analytical'
)
BEGIN
    CREATE NONCLUSTERED COLUMNSTORE INDEX CSI_Diagnostics_DiagnosticEventFact_Analytical
    ON Diagnostics.DiagnosticEventFact
    (
        DiagnosticEventId,
        DiagnosticEventDimensionId,
        TenantId,
        DateKey,
        Severity,
        DurationMs,
        EventCount,
        CreatedUtc
    );
    PRINT 'CSI_Diagnostics_DiagnosticEventFact_Analytical created.';
END
ELSE
    PRINT 'CSI_Diagnostics_DiagnosticEventFact_Analytical already exists -- skipped.';
GO

------------------------------------------------------------
-- Live verification: confirm the columnstore index exists, confirm the
-- pre-existing rowstore clustered PK and other nonclustered indexes on the
-- same table were NOT removed (OLTP access pattern preserved), and confirm
-- the table is still at 0 rows (no data touched by this structural change).
------------------------------------------------------------
SELECT i.name, i.type_desc, i.is_unique
FROM sys.indexes i
WHERE i.object_id = OBJECT_ID('Diagnostics.DiagnosticEventFact')
ORDER BY i.index_id;
GO

SELECT SUM(p.rows) AS DiagnosticEventFactRowCount
FROM sys.partitions p
WHERE p.object_id = OBJECT_ID('Diagnostics.DiagnosticEventFact')
  AND p.index_id IN (0, 1);
GO
