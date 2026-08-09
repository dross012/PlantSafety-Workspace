-- ============================================================================
--  Project:        Enterprise.WorkflowOrchestration -- Phase1-WorkflowOrchestration
--  File:           01-tables.sql
--  Created:        2026-08-09
--  Description:    Table DDL for [EnterpriseWorkflow].[WorkflowInstance]
--                  (StrongId WorkflowId, CurrentState per
--                  EnterpriseWorkflowState.cs), [EnterpriseWorkflow].
--                  [WorkflowStep] (the ordered execution plan --
--                  EnterpriseWorkflowStep.cs: StepType/Code/Description --
--                  built via EnterpriseWorkflowOrchestrator.BuildExecutionPlan),
--                  and [EnterpriseWorkflow].[WorkflowTransition] (append-only
--                  state-transition history -- EnterpriseWorkflowTransition.cs:
--                  WorkflowId/FromState/ToState/OccurredAtUtc).
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'WorkflowInstance' AND schema_id = SCHEMA_ID('EnterpriseWorkflow'))
BEGIN
    CREATE TABLE [EnterpriseWorkflow].[WorkflowInstance]
    (
        [WorkflowId]     UNIQUEIDENTIFIER  NOT NULL,
        [CurrentState]   NVARCHAR(20)      NOT NULL CONSTRAINT DF_WorkflowInstance_CurrentState DEFAULT (N'Pending'),
        [CreatedAtUtc]   DATETIME2(3)      NOT NULL,
        [UpdatedAtUtc]   DATETIME2(3)      NOT NULL,

        [CreatedUtc]     DATETIME2(3)      NOT NULL CONSTRAINT DF_WorkflowInstance_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]      NVARCHAR(128)     NOT NULL CONSTRAINT DF_WorkflowInstance_CreatedBy DEFAULT SYSTEM_USER,
        [UpdatedUtc]     DATETIME2(3)      NULL,
        [UpdatedBy]      NVARCHAR(128)     NULL,
        [IsDeleted]      BIT               NOT NULL CONSTRAINT DF_WorkflowInstance_IsDeleted DEFAULT (0),

        CONSTRAINT PK_WorkflowInstance PRIMARY KEY CLUSTERED ([WorkflowId]),
        CONSTRAINT CK_WorkflowInstance_CurrentState CHECK ([CurrentState] IN (N'Pending', N'InProgress', N'Completed', N'Failed', N'Cancelled'))
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'WorkflowStep' AND schema_id = SCHEMA_ID('EnterpriseWorkflow'))
BEGIN
    CREATE TABLE [EnterpriseWorkflow].[WorkflowStep]
    (
        [WorkflowStepId]   UNIQUEIDENTIFIER  NOT NULL CONSTRAINT DF_WorkflowStep_Id DEFAULT NEWSEQUENTIALID(),
        [WorkflowId]        UNIQUEIDENTIFIER  NOT NULL,
        [SequenceNumber]    INT               NOT NULL,
        [StepType]          NVARCHAR(20)      NOT NULL,
        [Code]               NVARCHAR(100)     NOT NULL,
        [Description]        NVARCHAR(500)     NOT NULL,

        CONSTRAINT PK_WorkflowStep PRIMARY KEY CLUSTERED ([WorkflowStepId]),
        CONSTRAINT UQ_WorkflowStep_WorkflowId_SequenceNumber UNIQUE ([WorkflowId], [SequenceNumber]),
        CONSTRAINT FK_WorkflowStep_WorkflowInstance FOREIGN KEY ([WorkflowId])
            REFERENCES [EnterpriseWorkflow].[WorkflowInstance] ([WorkflowId]),
        CONSTRAINT CK_WorkflowStep_StepType CHECK ([StepType] IN
            (N'RequiredAction', N'SopTrigger', N'PermitTrigger', N'EvacuationTrigger', N'ShutdownTrigger', N'RestartTrigger'))
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'WorkflowTransition' AND schema_id = SCHEMA_ID('EnterpriseWorkflow'))
BEGIN
    CREATE TABLE [EnterpriseWorkflow].[WorkflowTransition]
    (
        [WorkflowTransitionId]  UNIQUEIDENTIFIER  NOT NULL CONSTRAINT DF_WorkflowTransition_Id DEFAULT NEWSEQUENTIALID(),
        [WorkflowId]             UNIQUEIDENTIFIER  NOT NULL,
        [FromState]              NVARCHAR(20)      NOT NULL,
        [ToState]                NVARCHAR(20)      NOT NULL,
        [OccurredAtUtc]          DATETIME2(3)      NOT NULL,

        [CreatedUtc]             DATETIME2(3)      NOT NULL CONSTRAINT DF_WorkflowTransition_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]              NVARCHAR(128)     NOT NULL CONSTRAINT DF_WorkflowTransition_CreatedBy DEFAULT SYSTEM_USER,
        [IsDeleted]              BIT               NOT NULL CONSTRAINT DF_WorkflowTransition_IsDeleted DEFAULT (0),

        CONSTRAINT PK_WorkflowTransition PRIMARY KEY CLUSTERED ([WorkflowTransitionId]),
        CONSTRAINT FK_WorkflowTransition_WorkflowInstance FOREIGN KEY ([WorkflowId])
            REFERENCES [EnterpriseWorkflow].[WorkflowInstance] ([WorkflowId]),
        CONSTRAINT CK_WorkflowTransition_FromState CHECK ([FromState] IN (N'Pending', N'InProgress', N'Completed', N'Failed', N'Cancelled')),
        CONSTRAINT CK_WorkflowTransition_ToState CHECK ([ToState] IN (N'Pending', N'InProgress', N'Completed', N'Failed', N'Cancelled'))
    );
END
GO
