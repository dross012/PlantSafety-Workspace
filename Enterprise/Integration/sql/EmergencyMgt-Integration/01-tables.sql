-- =============================================
-- Enterprise.Integration -- EmergencyMgt-Integration
-- Script:      01-tables.sql
-- Purpose:     RoutingKeyRegistry, EmergencySummaryInbound, EmergencyLifecycleInbound,
--              EmergencyCorrectiveActionInbound, EmergencyReadinessInbound, EmergencyGovernanceLog.
-- Idempotent:  existence-checked CREATE. Safe to re-run.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Registry of routing keys EmergencyMgt publishes (see 40-seed-routingkeys.sql). PlantSafety
-- subscribes to these by name; this table is the local record of what is expected/registered,
-- used by usp_IntegrationHealth_Check / usp_ValidateEmergencyMgtIntegration below.
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'EmergencyMgtIntegration' AND t.name = 'RoutingKeyRegistry')
BEGIN
    CREATE TABLE [EmergencyMgtIntegration].[RoutingKeyRegistry] (
        [RoutingKeyRegistryId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_EmergencyMgtIntegration_RoutingKeyRegistry_Id DEFAULT NEWID(),
        [RoutingKey]           NVARCHAR(100) NOT NULL,
        [Description]          NVARCHAR(300) NULL,
        [IsActive]             BIT NOT NULL CONSTRAINT DF_EmergencyMgtIntegration_RoutingKeyRegistry_IsActive DEFAULT 1,
        [RegisteredUtc]        DATETIME2 NOT NULL CONSTRAINT DF_EmergencyMgtIntegration_RoutingKeyRegistry_RegisteredUtc DEFAULT SYSUTCDATETIME(),
        [LastSeenUtc]          DATETIME2 NULL,
        CONSTRAINT PK_EmergencyMgtIntegration_RoutingKeyRegistry PRIMARY KEY CLUSTERED ([RoutingKeyRegistryId]),
        CONSTRAINT UQ_EmergencyMgtIntegration_RoutingKeyRegistry_RoutingKey UNIQUE ([RoutingKey])
    );
END
GO

-- Read-only proxy feed of EmergencyMgt event summary data, pushed in by the EmergencyMgt
-- Servicer. Backs /integration/emergency/summary and /integration/emergency/kpis.
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'EmergencyMgtIntegration' AND t.name = 'EmergencySummaryInbound')
BEGIN
    CREATE TABLE [EmergencyMgtIntegration].[EmergencySummaryInbound] (
        [EmergencySummaryInboundId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_EmergencyMgtIntegration_EmergencySummaryInbound_Id DEFAULT NEWID(),
        [SourceEmergencyEventId]    UNIQUEIDENTIFIER NOT NULL,
        [EmergencyTypeName]         NVARCHAR(64) NOT NULL,
        [Severity]                  NVARCHAR(32) NOT NULL,
        [StatusCode]                NVARCHAR(32) NOT NULL,
        [IsStatusTerminal]          BIT NOT NULL,
        [Location]                  NVARCHAR(150) NULL,
        [InitiatedBy]               NVARCHAR(128) NULL,
        [StartedUtc]                DATETIME2 NOT NULL,
        [EndedUtc]                  DATETIME2 NULL,
        [ReceivedUtc]               DATETIME2 NOT NULL CONSTRAINT DF_EmergencyMgtIntegration_EmergencySummaryInbound_ReceivedUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_EmergencyMgtIntegration_EmergencySummaryInbound PRIMARY KEY CLUSTERED ([EmergencySummaryInboundId]),
        CONSTRAINT UQ_EmergencyMgtIntegration_EmergencySummaryInbound_SourceEmergencyEventId UNIQUE ([SourceEmergencyEventId])
    );
END
GO

-- Append-only feed of EmergencyMgt lifecycle transitions (Declared -> Active Response ->
-- Stabilizing -> Resolved -> Closed -> Archived). Backs /integration/emergency/lifecycle.
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'EmergencyMgtIntegration' AND t.name = 'EmergencyLifecycleInbound')
BEGIN
    CREATE TABLE [EmergencyMgtIntegration].[EmergencyLifecycleInbound] (
        [EmergencyLifecycleInboundId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_EmergencyMgtIntegration_EmergencyLifecycleInbound_Id DEFAULT NEWID(),
        [SourceEmergencyEventId]      UNIQUEIDENTIFIER NOT NULL,
        [FromStatus]                  NVARCHAR(32) NULL,
        [ToStatus]                    NVARCHAR(32) NOT NULL,
        [TransitionUtc]               DATETIME2 NOT NULL,
        [ReceivedUtc]                 DATETIME2 NOT NULL CONSTRAINT DF_EmergencyMgtIntegration_EmergencyLifecycleInbound_ReceivedUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_EmergencyMgtIntegration_EmergencyLifecycleInbound PRIMARY KEY CLUSTERED ([EmergencyLifecycleInboundId]),
        CONSTRAINT UQ_EmergencyMgtIntegration_EmergencyLifecycleInbound_Natural UNIQUE ([SourceEmergencyEventId], [ToStatus], [TransitionUtc])
    );
END
GO

-- Read-only proxy feed of EmergencyMgt corrective actions. Backs
-- /integration/emergency/corrective-actions.
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'EmergencyMgtIntegration' AND t.name = 'EmergencyCorrectiveActionInbound')
BEGIN
    CREATE TABLE [EmergencyMgtIntegration].[EmergencyCorrectiveActionInbound] (
        [EmergencyCorrectiveActionInboundId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_EmergencyMgtIntegration_EmergencyCorrectiveActionInbound_Id DEFAULT NEWID(),
        [SourceCorrectiveActionId]           UNIQUEIDENTIFIER NOT NULL,
        [SourceEmergencyEventId]             UNIQUEIDENTIFIER NULL,
        [Description]                        NVARCHAR(MAX) NOT NULL,
        [AssignedTo]                         NVARCHAR(128) NOT NULL,
        [DueUtc]                             DATETIME2 NULL,
        [StatusCode]                         NVARCHAR(32) NOT NULL,
        [ReceivedUtc]                        DATETIME2 NOT NULL CONSTRAINT DF_EmergencyMgtIntegration_EmergencyCorrectiveActionInbound_ReceivedUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_EmergencyMgtIntegration_EmergencyCorrectiveActionInbound PRIMARY KEY CLUSTERED ([EmergencyCorrectiveActionInboundId]),
        CONSTRAINT UQ_EmergencyMgtIntegration_EmergencyCorrectiveActionInbound_SourceId UNIQUE ([SourceCorrectiveActionId])
    );
END
GO

-- Read-only proxy feed of EmergencyMgt readiness scores. Backs /integration/emergency/readiness
-- and /integration/emergency/kpis.
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'EmergencyMgtIntegration' AND t.name = 'EmergencyReadinessInbound')
BEGIN
    CREATE TABLE [EmergencyMgtIntegration].[EmergencyReadinessInbound] (
        [EmergencyReadinessInboundId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_EmergencyMgtIntegration_EmergencyReadinessInbound_Id DEFAULT NEWID(),
        [SourceReadinessScoreId]      UNIQUEIDENTIFIER NOT NULL,
        [OverallScore]                DECIMAL(5,2) NOT NULL,
        [FactorScoresJson]            NVARCHAR(MAX) NOT NULL,
        [ComputedUtc]                 DATETIME2 NOT NULL,
        [ReceivedUtc]                 DATETIME2 NOT NULL CONSTRAINT DF_EmergencyMgtIntegration_EmergencyReadinessInbound_ReceivedUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_EmergencyMgtIntegration_EmergencyReadinessInbound PRIMARY KEY CLUSTERED ([EmergencyReadinessInboundId]),
        CONSTRAINT UQ_EmergencyMgtIntegration_EmergencyReadinessInbound_SourceId UNIQUE ([SourceReadinessScoreId]),
        CONSTRAINT CK_EmergencyMgtIntegration_EmergencyReadinessInbound_FactorScoresJson CHECK (ISJSON([FactorScoresJson]) = 1)
    );
END
GO

-- Log of governance signals consumed from EmergencyMgt (version manifest, drift-check, health).
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'EmergencyMgtIntegration' AND t.name = 'EmergencyGovernanceLog')
BEGIN
    CREATE TABLE [EmergencyMgtIntegration].[EmergencyGovernanceLog] (
        [EmergencyGovernanceLogId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_EmergencyMgtIntegration_EmergencyGovernanceLog_Id DEFAULT NEWID(),
        [SignalType]      NVARCHAR(30) NOT NULL,  -- VersionManifest | DriftCheck | Health
        [PayloadJson]     NVARCHAR(MAX) NOT NULL,
        [ConsumedUtc]     DATETIME2 NOT NULL CONSTRAINT DF_EmergencyMgtIntegration_EmergencyGovernanceLog_ConsumedUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_EmergencyMgtIntegration_EmergencyGovernanceLog PRIMARY KEY CLUSTERED ([EmergencyGovernanceLogId]),
        CONSTRAINT CK_EmergencyMgtIntegration_EmergencyGovernanceLog_SignalType CHECK ([SignalType] IN (N'VersionManifest', N'DriftCheck', N'Health'))
    );
END
GO
