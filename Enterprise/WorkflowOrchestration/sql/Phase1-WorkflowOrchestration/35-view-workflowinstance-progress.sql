-- =============================================
-- Enterprise.WorkflowOrchestration -- Phase1-WorkflowOrchestration
-- Script:        35-view-workflowinstance-progress.sql
-- Purpose:       EnterpriseWorkflow.vw_WorkflowInstance_Progress -- current
--                state plus step count, one row per workflow instance.
-- Idempotent: CREATE OR ALTER. Safe to re-run.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER VIEW [EnterpriseWorkflow].[vw_WorkflowInstance_Progress]
AS
SELECT
    w.[WorkflowId], w.[CurrentState], w.[CreatedAtUtc], w.[UpdatedAtUtc],
    COUNT(s.[WorkflowStepId]) AS [StepCount]
FROM [EnterpriseWorkflow].[WorkflowInstance] AS w
LEFT JOIN [EnterpriseWorkflow].[WorkflowStep] AS s ON s.[WorkflowId] = w.[WorkflowId]
WHERE ISNULL(w.[IsDeleted], 0) = 0
GROUP BY w.[WorkflowId], w.[CurrentState], w.[CreatedAtUtc], w.[UpdatedAtUtc];
GO
