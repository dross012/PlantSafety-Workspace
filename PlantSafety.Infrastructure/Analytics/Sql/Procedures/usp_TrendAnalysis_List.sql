-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.TrendAnalysis
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_TrendAnalysis_List.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Analytics].[TrendAnalysis],
--                  backing PlantSafety.Application.Analytics.TrendAnalysis.Repositories.ITrendAnalysisRepository.GetTrendPointsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Analytics].[usp_TrendAnalysis_List]
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
FROM [Analytics].[TrendAnalysis]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [CalculatedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [CalculatedAtUtc] DESC;
GO
