-- =============================================
-- Shared.WorkflowEngine -- Phase1-WorkflowLog
-- Script:        35-view-workflowevaluationlog-latest.sql
-- Purpose:       WorkflowEngine.vw_WorkflowEvaluationLog_LatestByFacility --
--                each facility's most recently logged evaluation.
-- Idempotent: CREATE OR ALTER. Safe to re-run.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER VIEW [WorkflowEngine].[vw_WorkflowEvaluationLog_LatestByFacility]
AS
WITH Ranked AS (
    SELECT
        w.[WorkflowEvaluationLogId], w.[FacilityId], w.[HazardTypeCode], w.[SeverityCode],
        w.[AffectedScope], w.[EvaluatedAtUtc],
        ROW_NUMBER() OVER (PARTITION BY w.[FacilityId] ORDER BY w.[EvaluatedAtUtc] DESC) AS [RowNum]
    FROM [WorkflowEngine].[WorkflowEvaluationLog] AS w
    WHERE ISNULL(w.[IsDeleted], 0) = 0 AND w.[FacilityId] IS NOT NULL
)
SELECT [WorkflowEvaluationLogId], [FacilityId], [HazardTypeCode], [SeverityCode], [AffectedScope], [EvaluatedAtUtc]
FROM Ranked
WHERE [RowNum] = 1;
GO
