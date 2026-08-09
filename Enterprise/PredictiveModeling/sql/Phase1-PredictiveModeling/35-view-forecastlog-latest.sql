-- =============================================
-- Enterprise.PredictiveModeling -- Phase1-PredictiveModeling
-- Script:        35-view-forecastlog-latest.sql
-- Purpose:       EnterpriseForecast.vw_ForecastLog_LatestByKind.
-- Idempotent: CREATE OR ALTER. Safe to re-run.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER VIEW [EnterpriseForecast].[vw_ForecastLog_LatestByKind]
AS
WITH Ranked AS (
    SELECT
        f.[ForecastLogId], f.[ForecastKind], f.[MovingAverage], f.[TrendSlope],
        f.[ForecastedValue], f.[IsAnomalousSpike], f.[IsEscalating], f.[GeneratedAtUtc],
        ROW_NUMBER() OVER (PARTITION BY f.[ForecastKind] ORDER BY f.[GeneratedAtUtc] DESC) AS [RowNum]
    FROM [EnterpriseForecast].[ForecastLog] AS f
    WHERE ISNULL(f.[IsDeleted], 0) = 0
)
SELECT [ForecastLogId], [ForecastKind], [MovingAverage], [TrendSlope], [ForecastedValue], [IsAnomalousSpike], [IsEscalating], [GeneratedAtUtc]
FROM Ranked
WHERE [RowNum] = 1;
GO
