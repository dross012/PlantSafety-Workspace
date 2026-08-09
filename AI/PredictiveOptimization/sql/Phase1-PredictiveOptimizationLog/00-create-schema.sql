-- ============================================================================
--  Project:        AI.PredictiveOptimization
--  Module:         Phase1-PredictiveOptimizationLog
--  File:           00-create-schema.sql
--  Created:        2026-08-09
--  Description:    Creates the [AIPredictiveOptimization] schema --
--                  audit-trail log of every real PredictiveOptimizationEngine.
--                  Optimize(PredictiveOptimizationInput) call (Phase E.6).
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

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'AIPredictiveOptimization')
BEGIN
    EXEC('CREATE SCHEMA [AIPredictiveOptimization] AUTHORIZATION dbo;');
END
GO
