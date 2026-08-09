-- ============================================================================
--  Project:        AI.RegulatoryInterpretation
--  Module:         Phase1-RegulatoryInterpretationLog
--  File:           00-create-schema.sql
--  Created:        2026-08-09
--  Description:    Creates the [AIRegulatoryInterpretation] schema --
--                  audit-trail log of every real RegulatoryInterpretationEngine.
--                  Interpret(RegulatoryInterpretationInput) call (Phase E.4).
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

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'AIRegulatoryInterpretation')
BEGIN
    EXEC('CREATE SCHEMA [AIRegulatoryInterpretation] AUTHORIZATION dbo;');
END
GO
