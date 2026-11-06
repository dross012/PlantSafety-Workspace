-- =============================================
-- Enterprise.Integration -- EmergencyMgt-Integration
-- Script: 10-indexes.sql
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EmergencyMgtIntegration_EmergencySummaryInbound_StartedUtc' AND object_id = OBJECT_ID(N'[EmergencyMgtIntegration].[EmergencySummaryInbound]'))
    CREATE NONCLUSTERED INDEX IX_EmergencyMgtIntegration_EmergencySummaryInbound_StartedUtc
        ON [EmergencyMgtIntegration].[EmergencySummaryInbound] ([StartedUtc] DESC) INCLUDE ([IsStatusTerminal], [Severity]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EmergencyMgtIntegration_EmergencyLifecycleInbound_SourceEmergencyEventId' AND object_id = OBJECT_ID(N'[EmergencyMgtIntegration].[EmergencyLifecycleInbound]'))
    CREATE NONCLUSTERED INDEX IX_EmergencyMgtIntegration_EmergencyLifecycleInbound_SourceEmergencyEventId
        ON [EmergencyMgtIntegration].[EmergencyLifecycleInbound] ([SourceEmergencyEventId], [TransitionUtc] DESC);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EmergencyMgtIntegration_EmergencyCorrectiveActionInbound_StatusCode' AND object_id = OBJECT_ID(N'[EmergencyMgtIntegration].[EmergencyCorrectiveActionInbound]'))
    CREATE NONCLUSTERED INDEX IX_EmergencyMgtIntegration_EmergencyCorrectiveActionInbound_StatusCode
        ON [EmergencyMgtIntegration].[EmergencyCorrectiveActionInbound] ([StatusCode]) INCLUDE ([DueUtc], [AssignedTo]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EmergencyMgtIntegration_EmergencyReadinessInbound_ComputedUtc' AND object_id = OBJECT_ID(N'[EmergencyMgtIntegration].[EmergencyReadinessInbound]'))
    CREATE NONCLUSTERED INDEX IX_EmergencyMgtIntegration_EmergencyReadinessInbound_ComputedUtc
        ON [EmergencyMgtIntegration].[EmergencyReadinessInbound] ([ComputedUtc] DESC);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EmergencyMgtIntegration_EmergencyGovernanceLog_SignalType' AND object_id = OBJECT_ID(N'[EmergencyMgtIntegration].[EmergencyGovernanceLog]'))
    CREATE NONCLUSTERED INDEX IX_EmergencyMgtIntegration_EmergencyGovernanceLog_SignalType
        ON [EmergencyMgtIntegration].[EmergencyGovernanceLog] ([SignalType], [ConsumedUtc] DESC);
GO
