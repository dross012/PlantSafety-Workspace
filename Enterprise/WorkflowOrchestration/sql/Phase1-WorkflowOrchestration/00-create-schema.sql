-- ============================================================================
--  Project:        Enterprise.WorkflowOrchestration
--  Module:         Phase1-WorkflowOrchestration
--  File:           00-create-schema.sql
--  Created:        2026-08-09
--  Description:    Creates the [EnterpriseWorkflow] schema -- durable
--                  storage for orchestrated enterprise workflow instances,
--                  their coordinated steps (Phase D.6 EnterpriseWorkflowStep),
--                  and lifecycle-state transitions
--                  (EnterpriseWorkflowTransition), currently in-memory only.
--  DATABASE PLACEMENT: [PlantSafety.CoreDB] -- see Shared.FacilityDomain's
--  Phase1-Lookup DATABASE PLACEMENT ASSUMPTION note.
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'EnterpriseWorkflow')
BEGIN
    EXEC('CREATE SCHEMA [EnterpriseWorkflow] AUTHORIZATION dbo;');
END
GO
