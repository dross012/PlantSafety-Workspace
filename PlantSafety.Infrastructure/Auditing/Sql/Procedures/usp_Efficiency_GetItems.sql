-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Auditing.AuditEngagement.Efficiency
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Efficiency_GetItems.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read procedure backing IEfficiencyRepository.GetItemsAsync. Thin,
--                  discipline-scoped projection over the shared
--                  [Auditing].[AuditCategoryRecord] table, filtered to the 'Efficiency'
--                  discipline (Efficiency Audits).
--                  Additive-only evolution.
--  UPDATED:        2026-08-02 (Phase 6 query-plan-hint pass) -- added
--                  OPTION (OPTIMIZE FOR UNKNOWN) per
--                  ..\..\Analytics\UnifiedModel\Sql\query-plan-hint-convention.md: the optional
--                  @SiteId filter's selectivity varies (all-sites vs one-site), and this pattern
--                  is shared identically across all eight *_GetItems procs over
--                  [Auditing].[AuditCategoryRecord]; OPTIMIZE FOR UNKNOWN gives a stable
--                  average-selectivity estimate at zero recompile cost.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Auditing].[usp_Efficiency_GetItems]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
SELECT
    [ItemId], [AuditEngagementId], [TenantId], [SiteId], [Title], [Status], [RecordedAtUtc]
FROM [Auditing].[AuditCategoryRecord]
WHERE [Discipline] = N'Efficiency'
  AND [TenantId]    = @TenantId
  AND (@SiteId IS NULL OR [SiteId] = @SiteId)
  AND [RecordedAtUtc] BETWEEN @FromUtc AND @ToUtc
  AND ISNULL([IsDeleted], 0) = 0
ORDER BY [RecordedAtUtc] DESC
OPTION (OPTIMIZE FOR UNKNOWN);
GO
