-- ============================================================================
--  Project:        Enterprise.Integration -- Phase1-Integration
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

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FieldMapping_SourceSystem' AND object_id = OBJECT_ID('EnterpriseIntegration.FieldMapping'))
    CREATE NONCLUSTERED INDEX IX_FieldMapping_SourceSystem ON [EnterpriseIntegration].[FieldMapping] ([SourceSystem]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_IntegrationEventLog_SourceSystem_OccurredAtUtc' AND object_id = OBJECT_ID('EnterpriseIntegration.IntegrationEventLog'))
    CREATE NONCLUSTERED INDEX IX_IntegrationEventLog_SourceSystem_OccurredAtUtc
        ON [EnterpriseIntegration].[IntegrationEventLog] ([SourceSystem], [OccurredAtUtc] DESC);
GO
