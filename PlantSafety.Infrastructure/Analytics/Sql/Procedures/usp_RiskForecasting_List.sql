-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Predictive.RiskForecasting
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_RiskForecasting_List.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Analytics].[RiskForecasting],
--                  backing IRiskForecastingRepository.GetRiskForecastsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Analytics].[usp_RiskForecasting_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [ForecastId],
        [TenantId],
        [SiteId],
        [RiskCategory],
        [ForecastValue],
        [ForecastHorizonDays],
        [Status],
        [ForecastedAtUtc]
FROM [Analytics].[RiskForecasting]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [ForecastedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [ForecastedAtUtc] DESC;
GO
