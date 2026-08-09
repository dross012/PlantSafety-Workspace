-- ============================================================================
--  Project:        AI.RegulatoryInterpretation -- Phase1-RegulatoryInterpretationLog
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

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RegulatoryInterpretationLog_HazardTypeCode_GeneratedAtUtc' AND object_id = OBJECT_ID('AIRegulatoryInterpretation.RegulatoryInterpretationLog'))
    CREATE NONCLUSTERED INDEX IX_RegulatoryInterpretationLog_HazardTypeCode_GeneratedAtUtc
        ON [AIRegulatoryInterpretation].[RegulatoryInterpretationLog] ([InputHazardTypeCode], [GeneratedAtUtc] DESC);
GO
