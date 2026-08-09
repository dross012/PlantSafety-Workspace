-- ============================================================================
--  Project:        AI.Reasoning -- Phase1-ReasoningLog
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

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ReasoningSessionLog_ReasoningSessionId_EvaluatedAtUtc' AND object_id = OBJECT_ID('AIReasoning.ReasoningSessionLog'))
    CREATE NONCLUSTERED INDEX IX_ReasoningSessionLog_ReasoningSessionId_EvaluatedAtUtc
        ON [AIReasoning].[ReasoningSessionLog] ([ReasoningSessionId], [EvaluatedAtUtc] DESC);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ReasoningSessionLog_EvaluatedAtUtc' AND object_id = OBJECT_ID('AIReasoning.ReasoningSessionLog'))
    CREATE NONCLUSTERED INDEX IX_ReasoningSessionLog_EvaluatedAtUtc
        ON [AIReasoning].[ReasoningSessionLog] ([EvaluatedAtUtc] DESC);
GO
