-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Predictive.IncidentPrediction
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_IncidentPrediction_List.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Analytics].[IncidentPrediction],
--                  backing IIncidentPredictionRepository.GetIncidentPredictionsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Analytics].[usp_IncidentPrediction_List]
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
        [IncidentCategory],
        [PredictedProbability],
        [RiskLevel],
        [Status],
        [PredictedAtUtc]
FROM [Analytics].[IncidentPrediction]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [PredictedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [PredictedAtUtc] DESC;
GO
