-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Intelligence.PredictiveSafety
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_PredictiveSafety_GetPredictions.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IPredictiveSafetyRepository.GetPredictionsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Intelligence].[usp_PredictiveSafety_GetPredictions]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        PredictionId,
        TenantId,
        SiteId,
        RiskCategory,
        PredictedRiskScore,
        ModelVersion,
        Status,
        PredictedAtUtc,
        GeneratedBy
    FROM [Intelligence].[PredictiveSafety]
    WHERE TenantId = @TenantId
      AND IsDeleted = 0
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND PredictedAtUtc >= @FromUtc
      AND PredictedAtUtc <= @ToUtc
    ORDER BY PredictedAtUtc DESC;
END
GO
