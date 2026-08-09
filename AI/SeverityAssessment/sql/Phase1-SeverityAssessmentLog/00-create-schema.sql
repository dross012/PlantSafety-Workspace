-- ============================================================================
--  Project:        AI.SeverityAssessment
--  Module:         Phase1-SeverityAssessmentLog
--  File:           00-create-schema.sql
--  Created:        2026-08-09
--  Description:    Creates the [AISeverityAssessment] schema -- audit-trail
--                  log of every real SeverityAssessmentEngine.Assess call
--                  (Phase E.5).
--  DATABASE PLACEMENT: [PlantSafety.CoreDB] -- see AI.Reasoning's
--  Phase1-ReasoningLog note.
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'AISeverityAssessment')
BEGIN
    EXEC('CREATE SCHEMA [AISeverityAssessment] AUTHORIZATION dbo;');
END
GO
