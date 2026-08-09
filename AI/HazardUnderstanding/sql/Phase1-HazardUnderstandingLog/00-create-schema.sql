-- ============================================================================
--  Project:        AI.HazardUnderstanding
--  Module:         Phase1-HazardUnderstandingLog
--  File:           00-create-schema.sql
--  Created:        2026-08-09
--  Description:    Creates the [AIHazardUnderstanding] schema -- an
--                  audit-trail log of every real HazardUnderstandingEngine.
--                  Understand(HazardUnderstandingInput) call (Phase E.2).
--  DATABASE PLACEMENT: [PlantSafety.CoreDB] -- see AI.Reasoning's
--  Phase1-ReasoningLog\00-create-schema.sql note.
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'AIHazardUnderstanding')
BEGIN
    EXEC('CREATE SCHEMA [AIHazardUnderstanding] AUTHORIZATION dbo;');
END
GO
