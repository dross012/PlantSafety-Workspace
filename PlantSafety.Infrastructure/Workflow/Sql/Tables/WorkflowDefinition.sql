-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Workflow
--  Layer:          Infrastructure -- SQL Scripts
--  File:           WorkflowDefinition.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Tables backing the WorkflowDefinition aggregate root and its child
--                  WorkflowStepDefinition / WorkflowTransition graph nodes/edges. Tenant-scoped
--                  (TenantId NOT NULL uniqueidentifier) so the platform-wide Row-Level-Security
--                  discovery picks it up automatically. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Workflow')
    EXEC('CREATE SCHEMA Workflow AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'WorkflowDefinition' AND schema_id = SCHEMA_ID('Workflow'))
BEGIN
    CREATE TABLE [Workflow].[WorkflowDefinition]
    (
        WorkflowDefinitionId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_WorkflowDefinition PRIMARY KEY,
        TenantId             UNIQUEIDENTIFIER NOT NULL,
        SiteId               UNIQUEIDENTIFIER NULL,
        WorkflowType         NVARCHAR(30)     NOT NULL,
        Name                 NVARCHAR(200)    NOT NULL,
        Description          NVARCHAR(2000)   NOT NULL CONSTRAINT DF_WorkflowDefinition_Description DEFAULT (N''),
        Version              INT              NOT NULL CONSTRAINT DF_WorkflowDefinition_Version DEFAULT (1),
        IsActive             BIT              NOT NULL CONSTRAINT DF_WorkflowDefinition_IsActive DEFAULT (0),
        CreatedAtUtc         DATETIME2(3)     NOT NULL,
        CreatedBy            UNIQUEIDENTIFIER NOT NULL,
        IsDeleted            BIT              NOT NULL CONSTRAINT DF_WorkflowDefinition_IsDeleted DEFAULT (0),
        RowVersion           ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_WorkflowDefinition_TenantId_Active ON [Workflow].[WorkflowDefinition] (TenantId, IsActive, IsDeleted);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'WorkflowStepDefinition' AND schema_id = SCHEMA_ID('Workflow'))
BEGIN
    CREATE TABLE [Workflow].[WorkflowStepDefinition]
    (
        StepId               UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_WorkflowStepDefinition PRIMARY KEY,
        WorkflowDefinitionId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_WorkflowStepDefinition_Definition
            REFERENCES [Workflow].[WorkflowDefinition] (WorkflowDefinitionId),
        [Order]              INT              NOT NULL,
        Name                 NVARCHAR(200)    NOT NULL,
        StepType             NVARCHAR(30)     NOT NULL,
        AssignedRoleCode     NVARCHAR(100)    NULL,
        AssignedUserId       UNIQUEIDENTIFIER NULL,
        EscalationRoleCode   NVARCHAR(100)    NULL,
        EscalationUserId     UNIQUEIDENTIFIER NULL,
        EntryConditionField  NVARCHAR(100)    NULL,
        EntryConditionOperator NVARCHAR(20)   NULL,
        EntryConditionValue  NVARCHAR(400)    NULL,
        TimeoutSeconds       INT              NULL,
        TimeoutAction        NVARCHAR(20)     NULL,
        RetryMaxAttempts     INT              NULL,
        RetryBackoffStrategy NVARCHAR(20)     NULL,
        RetryBaseDelaySeconds INT             NULL
    );

    CREATE NONCLUSTERED INDEX IX_WorkflowStepDefinition_DefinitionId ON [Workflow].[WorkflowStepDefinition] (WorkflowDefinitionId, [Order]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'WorkflowTransition' AND schema_id = SCHEMA_ID('Workflow'))
BEGIN
    CREATE TABLE [Workflow].[WorkflowTransition]
    (
        WorkflowTransitionId  UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_WorkflowTransition PRIMARY KEY CONSTRAINT DF_WorkflowTransition_Id DEFAULT (NEWID()),
        WorkflowDefinitionId  UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_WorkflowTransition_Definition
            REFERENCES [Workflow].[WorkflowDefinition] (WorkflowDefinitionId),
        FromStepId            UNIQUEIDENTIFIER NOT NULL,
        ToStepId              UNIQUEIDENTIFIER NOT NULL,
        GuardField             NVARCHAR(100)    NULL,
        GuardOperator           NVARCHAR(20)     NULL,
        GuardExpectedValue      NVARCHAR(400)    NULL,
        IsDefault             BIT              NOT NULL CONSTRAINT DF_WorkflowTransition_IsDefault DEFAULT (0)
    );

    CREATE NONCLUSTERED INDEX IX_WorkflowTransition_DefinitionId_From ON [Workflow].[WorkflowTransition] (WorkflowDefinitionId, FromStepId);
END
GO
