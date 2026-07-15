-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Culture.TrendModeling
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_TrendModeling_List.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Analytics].[TrendModeling],
--                  backing ITrendModelingRepository.GetTrendModelsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Analytics].[usp_TrendModeling_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [TrendModelId],
        [TenantId],
        [SiteId],
        [MetricName],
        [PredictedValue],
        [ModelConfidence],
        [Status],
        [GeneratedAtUtc]
FROM [Analytics].[TrendModeling]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [GeneratedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [GeneratedAtUtc] DESC;
GO
