-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Workflow
--  Layer:          Infrastructure -- SQL Scripts
--  File:           030-workflow-graph-foreign-keys.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (SQL & Database Agent, Phase 5 hardening pass)
--  SchemaVersion:  2026.1
--  Description:    Adds the missing referential-integrity FOREIGN KEY constraints within the
--                  Workflow step-graph: WorkflowInstance.CurrentStepId and
--                  WorkflowStepInstance.StepId both point at a WorkflowStepDefinition.StepId,
--                  and WorkflowTransition.FromStepId/ToStepId (the graph "edges") both point at
--                  a WorkflowStepDefinition.StepId within the same WorkflowDefinition -- none of
--                  these were declared as FKs in WorkflowDefinition.sql/WorkflowInstance.sql,
--                  so a step graph could reference nonexistent/foreign steps undetected. All
--                  four columns are NULL-able or point to a value that is always populated from
--                  a real step at write time in the corresponding usp_*_Insert/Upsert
--                  procedures, so adding NOT VALIDATED-safe FKs here is purely additive and
--                  matches the existing NULL-ability of each column
--                  (WorkflowInstance.CurrentStepId is NULL-able; WorkflowStepInstance.StepId,
--                  WorkflowTransition.FromStepId/ToStepId are NOT NULL). Idempotent (guarded by
--                  sys.foreign_keys existence checks); additive-only.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_WorkflowInstance_CurrentStep')
    ALTER TABLE [Workflow].[WorkflowInstance] WITH CHECK ADD CONSTRAINT FK_WorkflowInstance_CurrentStep FOREIGN KEY (CurrentStepId) REFERENCES [Workflow].[WorkflowStepDefinition] (StepId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_WorkflowStepInstance_StepDefinition')
    ALTER TABLE [Workflow].[WorkflowStepInstance] WITH CHECK ADD CONSTRAINT FK_WorkflowStepInstance_StepDefinition FOREIGN KEY (StepId) REFERENCES [Workflow].[WorkflowStepDefinition] (StepId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_WorkflowTransition_FromStep')
    ALTER TABLE [Workflow].[WorkflowTransition] WITH CHECK ADD CONSTRAINT FK_WorkflowTransition_FromStep FOREIGN KEY (FromStepId) REFERENCES [Workflow].[WorkflowStepDefinition] (StepId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_WorkflowTransition_ToStep')
    ALTER TABLE [Workflow].[WorkflowTransition] WITH CHECK ADD CONSTRAINT FK_WorkflowTransition_ToStep FOREIGN KEY (ToStepId) REFERENCES [Workflow].[WorkflowStepDefinition] (StepId);
GO
