-- ============================================================================
--  Project:        AI.HazardUnderstanding -- Phase1-HazardUnderstandingLog
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

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_HazardUnderstandingLog_HazardTypeCode_GeneratedAtUtc' AND object_id = OBJECT_ID('AIHazardUnderstanding.HazardUnderstandingLog'))
    CREATE NONCLUSTERED INDEX IX_HazardUnderstandingLog_HazardTypeCode_GeneratedAtUtc
        ON [AIHazardUnderstanding].[HazardUnderstandingLog] ([InputHazardTypeCode], [GeneratedAtUtc] DESC);
GO
