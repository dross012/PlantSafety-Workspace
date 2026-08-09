-- ============================================================================
--  Project:        Enterprise.WorkflowOrchestration -- Phase1-WorkflowOrchestration
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

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_WorkflowInstance_CurrentState' AND object_id = OBJECT_ID('EnterpriseWorkflow.WorkflowInstance'))
    CREATE NONCLUSTERED INDEX IX_WorkflowInstance_CurrentState ON [EnterpriseWorkflow].[WorkflowInstance] ([CurrentState]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_WorkflowStep_WorkflowId' AND object_id = OBJECT_ID('EnterpriseWorkflow.WorkflowStep'))
    CREATE NONCLUSTERED INDEX IX_WorkflowStep_WorkflowId ON [EnterpriseWorkflow].[WorkflowStep] ([WorkflowId], [SequenceNumber]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_WorkflowTransition_WorkflowId_OccurredAtUtc' AND object_id = OBJECT_ID('EnterpriseWorkflow.WorkflowTransition'))
    CREATE NONCLUSTERED INDEX IX_WorkflowTransition_WorkflowId_OccurredAtUtc
        ON [EnterpriseWorkflow].[WorkflowTransition] ([WorkflowId], [OccurredAtUtc] DESC);
GO
