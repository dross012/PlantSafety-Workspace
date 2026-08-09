-- ============================================================================
--  Project:        AI.WorkflowAutomation -- Phase1-WorkflowAutomationLog
--  File:           01-table-workflowautomationlog.sql
--  Created:        2026-08-09
--  Description:    Table DDL for [AIWorkflowAutomation].[WorkflowAutomationLog]
--                  -- one row per real WorkflowAutomationEngine.Automate call.
--                  Every output list (RequiredActions/SopTriggers/
--                  PermitTriggers/EvacuationTriggers/ShutdownRestartSequence/
--                  ExecutionPlan -- WorkflowAutomationOutput.cs) serialized
--                  as its own JSON column, per this task's instruction --
--                  mirrors Shared.WorkflowEngine's Phase1-WorkflowLog JSON
--                  pattern for the same underlying WorkflowDecision shape,
--                  extended with the two new Phase E.3 lists.
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'WorkflowAutomationLog' AND schema_id = SCHEMA_ID('AIWorkflowAutomation'))
BEGIN
    CREATE TABLE [AIWorkflowAutomation].[WorkflowAutomationLog]
    (
        [WorkflowAutomationLogId]   UNIQUEIDENTIFIER  NOT NULL CONSTRAINT DF_WorkflowAutomationLog_Id DEFAULT NEWSEQUENTIALID(),
        [RequiredActionsJson]        NVARCHAR(MAX)     NOT NULL,
        [SopStepsJson]               NVARCHAR(MAX)     NOT NULL,
        [PermitRequirementsJson]     NVARCHAR(MAX)     NOT NULL,
        [EvacuationStepsJson]        NVARCHAR(MAX)     NOT NULL,
        [ShutdownRestartSequenceJson] NVARCHAR(MAX)    NOT NULL,
        [ExecutionPlanJson]          NVARCHAR(MAX)     NOT NULL,
        [GeneratedAtUtc]             DATETIME2(3)      NOT NULL,

        [CreatedUtc]                 DATETIME2(3)      NOT NULL CONSTRAINT DF_WorkflowAutomationLog_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]                  NVARCHAR(128)     NOT NULL CONSTRAINT DF_WorkflowAutomationLog_CreatedBy DEFAULT SYSTEM_USER,
        [IsDeleted]                  BIT               NOT NULL CONSTRAINT DF_WorkflowAutomationLog_IsDeleted DEFAULT (0),

        CONSTRAINT PK_WorkflowAutomationLog PRIMARY KEY CLUSTERED ([WorkflowAutomationLogId]),
        CONSTRAINT CK_WorkflowAutomationLog_RequiredActionsJson CHECK (ISJSON([RequiredActionsJson]) = 1),
        CONSTRAINT CK_WorkflowAutomationLog_SopStepsJson CHECK (ISJSON([SopStepsJson]) = 1),
        CONSTRAINT CK_WorkflowAutomationLog_PermitRequirementsJson CHECK (ISJSON([PermitRequirementsJson]) = 1),
        CONSTRAINT CK_WorkflowAutomationLog_EvacuationStepsJson CHECK (ISJSON([EvacuationStepsJson]) = 1),
        CONSTRAINT CK_WorkflowAutomationLog_ShutdownRestartSequenceJson CHECK (ISJSON([ShutdownRestartSequenceJson]) = 1),
        CONSTRAINT CK_WorkflowAutomationLog_ExecutionPlanJson CHECK (ISJSON([ExecutionPlanJson]) = 1)
    );
END
GO
