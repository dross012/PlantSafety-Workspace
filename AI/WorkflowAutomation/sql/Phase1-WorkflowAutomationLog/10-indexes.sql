-- ============================================================================
--  Project:        AI.WorkflowAutomation -- Phase1-WorkflowAutomationLog
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

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_WorkflowAutomationLog_GeneratedAtUtc' AND object_id = OBJECT_ID('AIWorkflowAutomation.WorkflowAutomationLog'))
    CREATE NONCLUSTERED INDEX IX_WorkflowAutomationLog_GeneratedAtUtc
        ON [AIWorkflowAutomation].[WorkflowAutomationLog] ([GeneratedAtUtc] DESC);
GO
