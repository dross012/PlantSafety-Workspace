-- ============================================================================
--  Project:        AI.EnterpriseOrchestration
--  Module:         Phase1-EnterpriseOrchestrationLog
--  File:           00-create-schema.sql
--  Created:        2026-08-09
--  Description:    Creates the [AIEnterpriseOrchestration] schema --
--                  audit-trail log of every real
--                  EnterpriseOrchestrationEngine.Orchestrate call (Phase
--                  E.9, the final Path E composition phase aggregating
--                  every per-facility Path E engine output across the whole
--                  enterprise).
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

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'AIEnterpriseOrchestration')
BEGIN
    EXEC('CREATE SCHEMA [AIEnterpriseOrchestration] AUTHORIZATION dbo;');
END
GO
