-- =============================================
-- Shared.PredictiveModels -- Phase1-PredictionLog
-- Script:        35-view-predictionrunlog-latest.sql
-- Purpose:       PredictiveModels.vw_PredictionRunLog_LatestByModelAndSubject.
-- Idempotent: CREATE OR ALTER. Safe to re-run.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER VIEW [PredictiveModels].[vw_PredictionRunLog_LatestByModelAndSubject]
AS
WITH Ranked AS (
    SELECT
        p.[PredictionRunLogId], p.[ModelKind], p.[SubjectId], p.[MovingAverage], p.[TrendSlope],
        p.[ZScoreOfLatest], p.[PredictedValue], p.[IsAnomalous], p.[IsEscalating], p.[RunAtUtc],
        ROW_NUMBER() OVER (PARTITION BY p.[ModelKind], p.[SubjectId] ORDER BY p.[RunAtUtc] DESC) AS [RowNum]
    FROM [PredictiveModels].[PredictionRunLog] AS p
    WHERE ISNULL(p.[IsDeleted], 0) = 0
)
SELECT [PredictionRunLogId], [ModelKind], [SubjectId], [MovingAverage], [TrendSlope],
       [ZScoreOfLatest], [PredictedValue], [IsAnomalous], [IsEscalating], [RunAtUtc]
FROM Ranked
WHERE [RowNum] = 1;
GO
