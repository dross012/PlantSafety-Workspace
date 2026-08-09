-- ============================================================================
--  Project:        AI.WorkflowAutomation
--  Module:         Phase1-WorkflowAutomationLog
--  File:           00-create-schema.sql
--  Created:        2026-08-09
--  Description:    Creates the [AIWorkflowAutomation] schema -- audit-trail
--                  log of every real WorkflowAutomationEngine.Automate call
--                  (Phase E.3), which composes Shared.WorkflowEngine's
--                  WorkflowDecision + new shutdown/restart rules +
--                  Enterprise.WorkflowOrchestration's execution plan.
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

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'AIWorkflowAutomation')
BEGIN
    EXEC('CREATE SCHEMA [AIWorkflowAutomation] AUTHORIZATION dbo;');
END
GO
