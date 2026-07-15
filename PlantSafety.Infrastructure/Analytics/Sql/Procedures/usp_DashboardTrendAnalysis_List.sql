-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Dashboard.TrendAnalysis
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_DashboardTrendAnalysis_List.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Analytics].[DashboardTrendAnalysis],
--                  backing PlantSafety.Application.Analytics.Dashboard.TrendAnalysis.Repositories.ITrendAnalysisRepository.GetTrendPointsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Analytics].[usp_DashboardTrendAnalysis_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [TrendPointId],
        [TenantId],
        [SiteId],
        [MetricName],
        [MetricValue],
        [TrendDirection],
        [Status],
        [CalculatedAtUtc]
FROM [Analytics].[DashboardTrendAnalysis]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [CalculatedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [CalculatedAtUtc] DESC;
GO
