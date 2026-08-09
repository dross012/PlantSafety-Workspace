-- ============================================================================
--  Project:        Shared.WorkflowEngine
--  Module:         Phase1-WorkflowLog
--  File:           00-create-schema.sql
--  Created:        2026-08-09
--  Description:    Creates the [WorkflowEngine] schema -- durable logging
--                  for Shared.WorkflowEngine.Abstractions.Engine.
--                  WorkflowEngine.Evaluate (Phase C.7), which is currently a
--                  pure in-memory function with no persistence. Every
--                  evaluation's input (WorkflowContext) and output
--                  (WorkflowDecision: RequiredActions/SopTriggers/
--                  PermitTriggers/EvacuationTriggers/NotificationRoutings)
--                  is durably logged as one append-only row per call, so the
--                  SQL layer is a faithful audit trail of every real
--                  workflow evaluation. Per this task's instruction: session/
--                  log table design, not a full normalized relational model
--                  of every nested record -- the four trigger/action list
--                  outputs are serialized as JSON columns (this codebase has
--                  no prior JSON-column precedent to follow exactly, so this
--                  is Shared.WorkflowEngine's own first application of that
--                  pattern, consistent with the instruction to prefer JSON
--                  over full normalization for nested list outputs).
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

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'WorkflowEngine')
BEGIN
    EXEC('CREATE SCHEMA [WorkflowEngine] AUTHORIZATION dbo;');
END
GO
