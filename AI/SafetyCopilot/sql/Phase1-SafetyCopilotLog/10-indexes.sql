-- ============================================================================
--  Project:        AI.SafetyCopilot -- Phase1-SafetyCopilotLog
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

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SafetyCopilotLog_Audience_GeneratedAtUtc' AND object_id = OBJECT_ID('AISafetyCopilot.SafetyCopilotLog'))
    CREATE NONCLUSTERED INDEX IX_SafetyCopilotLog_Audience_GeneratedAtUtc
        ON [AISafetyCopilot].[SafetyCopilotLog] ([Audience], [GeneratedAtUtc] DESC);
GO
