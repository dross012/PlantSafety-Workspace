-- =============================================
-- Enterprise.Integration -- IncidentMgt-Integration
-- Script: 10-indexes.sql
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_IncidentMgtIntegration_IncidentSummaryFeed_SeverityRank' AND object_id = OBJECT_ID(N'[IncidentMgtIntegration].[IncidentSummaryFeed]'))
    CREATE NONCLUSTERED INDEX IX_IncidentMgtIntegration_IncidentSummaryFeed_SeverityRank
        ON [IncidentMgtIntegration].[IncidentSummaryFeed] ([SeverityRank] DESC) INCLUDE ([IsStatusTerminal], [OccurredUtc]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_IncidentMgtIntegration_IncidentSummaryFeed_OccurredUtc' AND object_id = OBJECT_ID(N'[IncidentMgtIntegration].[IncidentSummaryFeed]'))
    CREATE NONCLUSTERED INDEX IX_IncidentMgtIntegration_IncidentSummaryFeed_OccurredUtc
        ON [IncidentMgtIntegration].[IncidentSummaryFeed] ([OccurredUtc] DESC);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_IncidentMgtIntegration_GovernanceConsumptionLog_SignalType' AND object_id = OBJECT_ID(N'[IncidentMgtIntegration].[GovernanceConsumptionLog]'))
    CREATE NONCLUSTERED INDEX IX_IncidentMgtIntegration_GovernanceConsumptionLog_SignalType
        ON [IncidentMgtIntegration].[GovernanceConsumptionLog] ([SignalType], [ConsumedUtc] DESC);
GO
