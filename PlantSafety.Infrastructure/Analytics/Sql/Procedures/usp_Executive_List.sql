-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Dashboard.Executive
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Executive_List.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Analytics].[Executive],
--                  backing IExecutiveRepository.GetExecutiveSummaryAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Analytics].[usp_Executive_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [SummaryId],
        [TenantId],
        [SiteId],
        [MetricName],
        [MetricValue],
        [TargetValue],
        [Status],
        [SnapshotAtUtc]
FROM [Analytics].[Executive]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [SnapshotAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [SnapshotAtUtc] DESC;
GO
