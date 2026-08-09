-- ============================================================================
--  Project:        AI.DigitalTwinLearning
--  Module:         Phase1-DigitalTwinLearningLog
--  File:           00-create-schema.sql
--  Created:        2026-08-09
--  Description:    Creates the [AIDigitalTwinLearning] schema -- audit-trail
--                  log of every real DigitalTwinLearningEngine.Learn call
--                  (Phase E.7).
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

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'AIDigitalTwinLearning')
BEGIN
    EXEC('CREATE SCHEMA [AIDigitalTwinLearning] AUTHORIZATION dbo;');
END
GO
