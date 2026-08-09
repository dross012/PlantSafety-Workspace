-- ============================================================================
--  Project:        Enterprise.RulesEngine
--  Module:         Phase1-RulesEngine
--  File:           00-create-schema.sql
--  Created:        2026-08-09
--  Description:    Creates the [EnterpriseRules] schema -- durable storage
--                  for Enterprise.RulesEngine.Abstractions.Rules.
--                  EnterpriseRule (definitions, currently in-memory only)
--                  and durable logging of Engine.EnterpriseRuleEvaluationResult
--                  (Phase D.5). Named [EnterpriseRules] rather than
--                  [RulesEngine] to avoid a schema/module name collision
--                  with Shared.WorkflowEngine's sibling [WorkflowEngine]
--                  schema and this task's separately-named "RulesEngine"
--                  module reference, both of which live in the same
--                  CoreDB placement.
--  DATABASE PLACEMENT: [PlantSafety.CoreDB] -- see Shared.FacilityDomain's
--  Phase1-Lookup DATABASE PLACEMENT ASSUMPTION note. NOTE: this is
--  Enterprise.RulesEngine (Path D.5), NOT the not-yet-provisioned
--  PlantSafety.WorkflowDB/RulesEngineDB referenced in the canonical
--  templates' inventory for a *different*, older "RulesEngine" concept --
--  no source module maps to those placeholder DB names as of this task
--  (see the No-Guessing Rule feedback memory); flagged, not populated.
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'EnterpriseRules')
BEGIN
    EXEC('CREATE SCHEMA [EnterpriseRules] AUTHORIZATION dbo;');
END
GO
