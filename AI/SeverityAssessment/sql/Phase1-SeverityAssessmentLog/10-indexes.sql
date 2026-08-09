-- ============================================================================
--  Project:        AI.SeverityAssessment -- Phase1-SeverityAssessmentLog
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

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SeverityAssessmentLog_FinalAssessedSeverityCode_AssessedAtUtc' AND object_id = OBJECT_ID('AISeverityAssessment.SeverityAssessmentLog'))
    CREATE NONCLUSTERED INDEX IX_SeverityAssessmentLog_FinalAssessedSeverityCode_AssessedAtUtc
        ON [AISeverityAssessment].[SeverityAssessmentLog] ([FinalAssessedSeverityCode], [AssessedAtUtc] DESC);
GO
