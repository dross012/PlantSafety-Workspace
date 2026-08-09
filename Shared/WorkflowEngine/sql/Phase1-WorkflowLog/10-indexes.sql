-- ============================================================================
--  Project:        Shared.WorkflowEngine -- Phase1-WorkflowLog
--  File:           10-indexes.sql
--  Created:        2026-08-09
--  Idempotent: guarded by sys.indexes existence checks.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_WorkflowEvaluationLog_FacilityId_EvaluatedAtUtc' AND object_id = OBJECT_ID('WorkflowEngine.WorkflowEvaluationLog'))
    CREATE NONCLUSTERED INDEX IX_WorkflowEvaluationLog_FacilityId_EvaluatedAtUtc
        ON [WorkflowEngine].[WorkflowEvaluationLog] ([FacilityId], [EvaluatedAtUtc] DESC);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_WorkflowEvaluationLog_EvaluatedAtUtc' AND object_id = OBJECT_ID('WorkflowEngine.WorkflowEvaluationLog'))
    CREATE NONCLUSTERED INDEX IX_WorkflowEvaluationLog_EvaluatedAtUtc
        ON [WorkflowEngine].[WorkflowEvaluationLog] ([EvaluatedAtUtc] DESC);
GO
