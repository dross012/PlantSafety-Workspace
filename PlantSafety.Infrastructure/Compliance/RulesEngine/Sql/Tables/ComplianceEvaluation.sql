-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.RulesEngine
--  Layer:          Infrastructure -- SQL Scripts
--  File:           ComplianceEvaluation.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Tables backing the ComplianceEvaluation aggregate root
--                  (IComplianceEvaluationRepository) and its ComplianceRuleResult child entity.
--                  Lives in a dedicated [ComplianceRulesEngine] schema, deliberately distinct
--                  from the pre-existing [Compliance] schema (application/software audit
--                  read-models) so the two never collide on table/procedure names.
--                  ComplianceRuleResult.ViolationsJson stores the rule's violation details as a
--                  JSON array rather than a further child table, since violations are always
--                  read/written as a whole with their owning rule result. Additive-only
--                  evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'ComplianceRulesEngine')
BEGIN
    EXEC('CREATE SCHEMA [ComplianceRulesEngine]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ComplianceEvaluation' AND schema_id = SCHEMA_ID('ComplianceRulesEngine'))
BEGIN
    CREATE TABLE [ComplianceRulesEngine].[ComplianceEvaluation] (
        [ComplianceEvaluationId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId]               UNIQUEIDENTIFIER NOT NULL,
        [SiteId]                 UNIQUEIDENTIFIER NULL,
        [TargetEntityType]       NVARCHAR(200)    NOT NULL,
        [TargetEntityId]         UNIQUEIDENTIFIER NOT NULL,
        [Status]                 NVARCHAR(30)     NOT NULL,
        [StartedAtUtc]           DATETIME2(3)     NOT NULL,
        [CompletedAtUtc]         DATETIME2(3)     NULL,
        [TriggeredBy]            UNIQUEIDENTIFIER NOT NULL,
        [CreatedAtUtc]           DATETIME2(3)     NOT NULL CONSTRAINT DF_ComplianceEvaluation_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        [IsDeleted]              BIT              NOT NULL CONSTRAINT DF_ComplianceEvaluation_IsDeleted DEFAULT (0)
    );

    CREATE NONCLUSTERED INDEX IX_ComplianceEvaluation_Tenant_Target
        ON [ComplianceRulesEngine].[ComplianceEvaluation] (TenantId, TargetEntityType, TargetEntityId, StartedAtUtc DESC);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ComplianceRuleResult' AND schema_id = SCHEMA_ID('ComplianceRulesEngine'))
BEGIN
    CREATE TABLE [ComplianceRulesEngine].[ComplianceRuleResult] (
        [ComplianceRuleResultId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [ComplianceEvaluationId] UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT FK_ComplianceRuleResult_Evaluation REFERENCES [ComplianceRulesEngine].[ComplianceEvaluation] (ComplianceEvaluationId),
        [RuleId]                 UNIQUEIDENTIFIER NOT NULL,
        [RuleCode]                NVARCHAR(200)   NOT NULL,
        [RuleName]                NVARCHAR(200)   NOT NULL,
        [Category]                NVARCHAR(30)    NOT NULL,
        [Passed]                  BIT             NOT NULL,
        [ViolationsJson]          NVARCHAR(MAX)   NOT NULL CONSTRAINT DF_ComplianceRuleResult_Violations DEFAULT (N'[]'),
        [EvaluatedAtUtc]          DATETIME2(3)    NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_ComplianceRuleResult_Evaluation
        ON [ComplianceRulesEngine].[ComplianceRuleResult] (ComplianceEvaluationId);
END
GO
