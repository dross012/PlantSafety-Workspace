-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.PredictiveAnalytics
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_PredictiveAnalytics_List.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Analytics].[PredictiveAnalytics],
--                  backing IPredictiveAnalyticsRepository.GetPredictionsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Analytics].[usp_PredictiveAnalytics_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [PredictionId],
        [TenantId],
        [SiteId],
        [ModelName],
        [Score],
        [Status],
        [GeneratedAtUtc],
        [GeneratedBy]
FROM [Analytics].[PredictiveAnalytics]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [GeneratedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [GeneratedAtUtc] DESC;
GO
