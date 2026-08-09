-- ============================================================================
--  Project:        Enterprise.RulesEngine -- Phase1-RulesEngine
--  File:           01-tables.sql
--  Created:        2026-08-09
--  Description:    Table DDL for [EnterpriseRules].[Rule] (mirrors
--                  EnterpriseRule.cs: RuleId/Name/RuleType/IsEnabled),
--                  [EnterpriseRules].[RuleCondition] (EnterpriseRuleCondition.cs:
--                  FactKey/Operator/ComparisonValue, AND semantics per rule)
--                  and [EnterpriseRules].[RuleAction] (EnterpriseRuleAction.cs:
--                  ActionCode/Description), plus the append-only
--                  [EnterpriseRules].[RuleEvaluationLog] durably logging
--                  every real EnterpriseRuleEvaluationResult (RuleId/
--                  RuleName/Matched/TriggeredActions -- serialized as JSON,
--                  per this task's instruction, since TriggeredActions is a
--                  nested list of the already-normalized RuleAction shape
--                  and duplicating a full join here would be redundant with
--                  RuleAction itself).
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Rule' AND schema_id = SCHEMA_ID('EnterpriseRules'))
BEGIN
    CREATE TABLE [EnterpriseRules].[Rule]
    (
        [RuleId]      UNIQUEIDENTIFIER  NOT NULL,
        [Name]        NVARCHAR(200)     NOT NULL,
        [RuleType]    NVARCHAR(20)      NOT NULL,
        [IsEnabled]   BIT               NOT NULL CONSTRAINT DF_Rule_IsEnabled DEFAULT (1),

        [CreatedUtc]  DATETIME2(3)      NOT NULL CONSTRAINT DF_Rule_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]   NVARCHAR(128)     NOT NULL CONSTRAINT DF_Rule_CreatedBy DEFAULT SYSTEM_USER,
        [UpdatedUtc]  DATETIME2(3)      NULL,
        [UpdatedBy]   NVARCHAR(128)     NULL,
        [IsDeleted]   BIT               NOT NULL CONSTRAINT DF_Rule_IsDeleted DEFAULT (0),

        CONSTRAINT PK_Rule PRIMARY KEY CLUSTERED ([RuleId]),
        CONSTRAINT CK_Rule_RuleType CHECK ([RuleType] IN (N'Hazard', N'Severity', N'Regulatory', N'Workflow', N'Escalation'))
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RuleCondition' AND schema_id = SCHEMA_ID('EnterpriseRules'))
BEGIN
    CREATE TABLE [EnterpriseRules].[RuleCondition]
    (
        [RuleConditionId]   UNIQUEIDENTIFIER  NOT NULL CONSTRAINT DF_RuleCondition_Id DEFAULT NEWSEQUENTIALID(),
        [RuleId]            UNIQUEIDENTIFIER  NOT NULL,
        [FactKey]           NVARCHAR(100)     NOT NULL,
        [Operator]          NVARCHAR(20)      NOT NULL,
        [ComparisonValue]   NVARCHAR(200)     NOT NULL,

        CONSTRAINT PK_RuleCondition PRIMARY KEY CLUSTERED ([RuleConditionId]),
        CONSTRAINT FK_RuleCondition_Rule FOREIGN KEY ([RuleId]) REFERENCES [EnterpriseRules].[Rule] ([RuleId]),
        CONSTRAINT CK_RuleCondition_Operator CHECK ([Operator] IN (N'Equals', N'NotEquals', N'GreaterThan', N'LessThan', N'Contains'))
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RuleAction' AND schema_id = SCHEMA_ID('EnterpriseRules'))
BEGIN
    CREATE TABLE [EnterpriseRules].[RuleAction]
    (
        [RuleActionId]  UNIQUEIDENTIFIER  NOT NULL CONSTRAINT DF_RuleAction_Id DEFAULT NEWSEQUENTIALID(),
        [RuleId]        UNIQUEIDENTIFIER  NOT NULL,
        [ActionCode]    NVARCHAR(50)      NOT NULL,
        [Description]   NVARCHAR(500)     NOT NULL,

        CONSTRAINT PK_RuleAction PRIMARY KEY CLUSTERED ([RuleActionId]),
        CONSTRAINT FK_RuleAction_Rule FOREIGN KEY ([RuleId]) REFERENCES [EnterpriseRules].[Rule] ([RuleId])
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RuleEvaluationLog' AND schema_id = SCHEMA_ID('EnterpriseRules'))
BEGIN
    CREATE TABLE [EnterpriseRules].[RuleEvaluationLog]
    (
        [RuleEvaluationLogId]  UNIQUEIDENTIFIER  NOT NULL CONSTRAINT DF_RuleEvaluationLog_Id DEFAULT NEWSEQUENTIALID(),
        [RuleId]                UNIQUEIDENTIFIER  NOT NULL,
        [RuleName]              NVARCHAR(200)     NOT NULL,
        [Matched]                BIT               NOT NULL,
        [TriggeredActionsJson]   NVARCHAR(MAX)     NOT NULL,
        [EvaluatedAtUtc]         DATETIME2(3)      NOT NULL,

        [CreatedUtc]             DATETIME2(3)      NOT NULL CONSTRAINT DF_RuleEvaluationLog_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]              NVARCHAR(128)     NOT NULL CONSTRAINT DF_RuleEvaluationLog_CreatedBy DEFAULT SYSTEM_USER,
        [IsDeleted]              BIT               NOT NULL CONSTRAINT DF_RuleEvaluationLog_IsDeleted DEFAULT (0),

        CONSTRAINT PK_RuleEvaluationLog PRIMARY KEY CLUSTERED ([RuleEvaluationLogId]),
        CONSTRAINT FK_RuleEvaluationLog_Rule FOREIGN KEY ([RuleId]) REFERENCES [EnterpriseRules].[Rule] ([RuleId]),
        CONSTRAINT CK_RuleEvaluationLog_TriggeredActionsJson CHECK (ISJSON([TriggeredActionsJson]) = 1)
    );
END
GO
