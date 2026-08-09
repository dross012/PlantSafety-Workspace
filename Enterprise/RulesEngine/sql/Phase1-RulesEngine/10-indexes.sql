-- ============================================================================
--  Project:        Enterprise.RulesEngine -- Phase1-RulesEngine
--  File:           10-indexes.sql
--  Created:        2026-08-09
--  Idempotent: guarded by sys.indexes existence checks.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Rule_RuleType_IsEnabled' AND object_id = OBJECT_ID('EnterpriseRules.Rule'))
    CREATE NONCLUSTERED INDEX IX_Rule_RuleType_IsEnabled ON [EnterpriseRules].[Rule] ([RuleType], [IsEnabled]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RuleCondition_RuleId' AND object_id = OBJECT_ID('EnterpriseRules.RuleCondition'))
    CREATE NONCLUSTERED INDEX IX_RuleCondition_RuleId ON [EnterpriseRules].[RuleCondition] ([RuleId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RuleAction_RuleId' AND object_id = OBJECT_ID('EnterpriseRules.RuleAction'))
    CREATE NONCLUSTERED INDEX IX_RuleAction_RuleId ON [EnterpriseRules].[RuleAction] ([RuleId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RuleEvaluationLog_RuleId_EvaluatedAtUtc' AND object_id = OBJECT_ID('EnterpriseRules.RuleEvaluationLog'))
    CREATE NONCLUSTERED INDEX IX_RuleEvaluationLog_RuleId_EvaluatedAtUtc
        ON [EnterpriseRules].[RuleEvaluationLog] ([RuleId], [EvaluatedAtUtc] DESC);
GO
