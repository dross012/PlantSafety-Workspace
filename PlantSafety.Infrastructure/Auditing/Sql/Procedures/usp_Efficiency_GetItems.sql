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
ORDER BY [RecordedAtUtc] DESC;
GO
