-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Workflow
--  Layer:          Infrastructure -- SQL Scripts
--  File:           WorkflowInstance.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Tables backing the WorkflowInstance aggregate root and its child
--                  WorkflowStepInstance execution history. Tenant-scoped (TenantId NOT NULL
--                  uniqueidentifier) so the platform-wide Row-Level-Security discovery picks it up
--                  automatically. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'WorkflowInstance' AND schema_id = SCHEMA_ID('Workflow'))
BEGIN
    CREATE TABLE [Workflow].[WorkflowInstance]
    (
        WorkflowInstanceId   UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_WorkflowInstance PRIMARY KEY,
        WorkflowDefinitionId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_WorkflowInstance_Definition
            REFERENCES [Workflow].[WorkflowDefinition] (WorkflowDefinitionId),
        TenantId             UNIQUEIDENTIFIER NOT NULL,
        SiteId               UNIQUEIDENTIFIER NULL,
        WorkflowType         NVARCHAR(30)     NOT NULL,
        State                NVARCHAR(30)     NOT NULL,
        CurrentStepId        UNIQUEIDENTIFIER NULL,
        ContextDataJson       NVARCHAR(MAX)    NOT NULL CONSTRAINT DF_WorkflowInstance_ContextData DEFAULT (N'{}'),
        StartedAtUtc         DATETIME2(3)     NOT NULL,
        CompletedAtUtc       DATETIME2(3)     NULL,
        InitiatedBy          UNIQUEIDENTIFIER NOT NULL,
        CorrelationId        UNIQUEIDENTIFIER NOT NULL,
        IsDeleted            BIT              NOT NULL CONSTRAINT DF_WorkflowInstance_IsDeleted DEFAULT (0),
        RowVersion           ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_WorkflowInstance_TenantId_State ON [Workflow].[WorkflowInstance] (TenantId, State, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_WorkflowInstance_DefinitionId ON [Workflow].[WorkflowInstance] (WorkflowDefinitionId);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'WorkflowStepInstance' AND schema_id = SCHEMA_ID('Workflow'))
BEGIN
    CREATE TABLE [Workflow].[WorkflowStepInstance]
    (
        StepInstanceId       UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_WorkflowStepInstance PRIMARY KEY,
        WorkflowInstanceId   UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_WorkflowStepInstance_Instance
            REFERENCES [Workflow].[WorkflowInstance] (WorkflowInstanceId),
        StepId               UNIQUEIDENTIFIER NOT NULL,
        StepType             NVARCHAR(30)     NOT NULL,
        Status               NVARCHAR(30)     NOT NULL,
        AssignedRoleCode     NVARCHAR(100)    NULL,
        AssignedUserId       UNIQUEIDENTIFIER NULL,
        ActivatedAtUtc       DATETIME2(3)     NULL,
        DecidedAtUtc         DATETIME2(3)     NULL,
        DecidedBy            UNIQUEIDENTIFIER NULL,
        DecisionNotes        NVARCHAR(2000)   NULL,
        AttemptCount         INT              NOT NULL CONSTRAINT DF_WorkflowStepInstance_AttemptCount DEFAULT (0),
        NextAttemptAtUtc     DATETIME2(3)     NULL,
        RecommendationReference NVARCHAR(400) NULL,
        SequenceNo           INT              NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_WorkflowStepInstance_InstanceId ON [Workflow].[WorkflowStepInstance] (WorkflowInstanceId, SequenceNo);
END
GO
