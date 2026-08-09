-- ============================================================================
--  Project:        AI.EnterpriseOrchestration -- Phase1-EnterpriseOrchestrationLog
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

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EnterpriseOrchestrationLog_GeneratedAtUtc' AND object_id = OBJECT_ID('AIEnterpriseOrchestration.EnterpriseOrchestrationLog'))
    CREATE NONCLUSTERED INDEX IX_EnterpriseOrchestrationLog_GeneratedAtUtc
        ON [AIEnterpriseOrchestration].[EnterpriseOrchestrationLog] ([GeneratedAtUtc] DESC);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EnterpriseOrchestrationLog_HighestSeverityCode' AND object_id = OBJECT_ID('AIEnterpriseOrchestration.EnterpriseOrchestrationLog'))
    CREATE NONCLUSTERED INDEX IX_EnterpriseOrchestrationLog_HighestSeverityCode
        ON [AIEnterpriseOrchestration].[EnterpriseOrchestrationLog] ([HighestSeverityCode]);
GO
