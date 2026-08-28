-- =============================================
-- Enterprise.Integration -- IncidentMgt-Integration
-- Script:      01-tables.sql
-- Purpose:     RoutingKeyRegistry, IncidentSummaryFeed, GovernanceConsumptionLog.
-- Idempotent:  existence-checked CREATE. Safe to re-run.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Registry of the 8 routing keys/topics IncidentMgt publishes (see IncidentMgt repo,
-- installer server spec, Section 6 "Define routing keys/topics"). PlantSafety subscribes to
-- these by name; this table is the local record of what is expected/registered, used by the
-- installer health check (see 40-seed-routingkeys.sql and the PlantSafety installer integration
-- checklist in the IncidentMgt repo's removal/integration spec doc).
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'IncidentMgtIntegration' AND t.name = 'RoutingKeyRegistry')
BEGIN
    CREATE TABLE [IncidentMgtIntegration].[RoutingKeyRegistry] (
        [RoutingKeyRegistryId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_IncidentMgtIntegration_RoutingKeyRegistry_Id DEFAULT NEWID(),
        [RoutingKey]           NVARCHAR(100) NOT NULL,
        [Description]          NVARCHAR(300) NULL,
        [IsActive]             BIT NOT NULL CONSTRAINT DF_IncidentMgtIntegration_RoutingKeyRegistry_IsActive DEFAULT 1,
        [RegisteredUtc]        DATETIME2 NOT NULL CONSTRAINT DF_IncidentMgtIntegration_RoutingKeyRegistry_RegisteredUtc DEFAULT SYSUTCDATETIME(),
        [LastSeenUtc]          DATETIME2 NULL,
        CONSTRAINT PK_IncidentMgtIntegration_RoutingKeyRegistry PRIMARY KEY CLUSTERED ([RoutingKeyRegistryId]),
        CONSTRAINT UQ_IncidentMgtIntegration_RoutingKeyRegistry_RoutingKey UNIQUE ([RoutingKey])
    );
END
GO

-- Read-only proxy feed of IncidentMgt summary/severity/risk data, pushed in by the IncidentMgt
-- Servicer. Backs "PlantSafety UI Integration" (Section 3.4) and "PlantSafety API Integration"
-- (Section 3.5: /plantsafety/incidents/summary, /trends, /risk).
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'IncidentMgtIntegration' AND t.name = 'IncidentSummaryFeed')
BEGIN
    CREATE TABLE [IncidentMgtIntegration].[IncidentSummaryFeed] (
        [IncidentSummaryFeedId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_IncidentMgtIntegration_IncidentSummaryFeed_Id DEFAULT NEWID(),
        [SourceIncidentId]      UNIQUEIDENTIFIER NOT NULL,
        [IncidentNumber]        NVARCHAR(30) NOT NULL,
        [Title]                 NVARCHAR(200) NOT NULL,
        [SeverityCode]          NVARCHAR(50) NOT NULL,
        [SeverityRank]          INT NOT NULL,
        [StatusCode]            NVARCHAR(30) NOT NULL,
        [IsStatusTerminal]      BIT NOT NULL,
        [CategoryName]          NVARCHAR(100) NULL,
        [LocationName]          NVARCHAR(150) NULL,
        [OccurredUtc]           DATETIME2 NOT NULL,
        [ClosedUtc]             DATETIME2 NULL,
        [RiskRating]            NVARCHAR(30) NULL,
        [InvestigationStatus]   NVARCHAR(30) NULL,
        [RootCauseSummary]      NVARCHAR(500) NULL,
        [CorrectiveActionStatus] NVARCHAR(30) NULL,
        [RegulatoryComplianceStatus] NVARCHAR(30) NULL,
        [ReceivedUtc]           DATETIME2 NOT NULL CONSTRAINT DF_IncidentMgtIntegration_IncidentSummaryFeed_ReceivedUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_IncidentMgtIntegration_IncidentSummaryFeed PRIMARY KEY CLUSTERED ([IncidentSummaryFeedId]),
        CONSTRAINT UQ_IncidentMgtIntegration_IncidentSummaryFeed_SourceIncidentId UNIQUE ([SourceIncidentId])
    );
END
GO

-- Log of governance signals consumed from IncidentMgt (version manifest, drift-check, health
-- report), per Section 3.6 "PlantSafety Governance Integration".
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'IncidentMgtIntegration' AND t.name = 'GovernanceConsumptionLog')
BEGIN
    CREATE TABLE [IncidentMgtIntegration].[GovernanceConsumptionLog] (
        [GovernanceConsumptionLogId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_IncidentMgtIntegration_GovernanceConsumptionLog_Id DEFAULT NEWID(),
        [SignalType]      NVARCHAR(30) NOT NULL,  -- VersionManifest | DriftCheck | Health
        [PayloadJson]     NVARCHAR(MAX) NOT NULL,
        [ConsumedUtc]     DATETIME2 NOT NULL CONSTRAINT DF_IncidentMgtIntegration_GovernanceConsumptionLog_ConsumedUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_IncidentMgtIntegration_GovernanceConsumptionLog PRIMARY KEY CLUSTERED ([GovernanceConsumptionLogId]),
        CONSTRAINT CK_IncidentMgtIntegration_GovernanceConsumptionLog_SignalType CHECK ([SignalType] IN (N'VersionManifest', N'DriftCheck', N'Health'))
    );
END
GO
