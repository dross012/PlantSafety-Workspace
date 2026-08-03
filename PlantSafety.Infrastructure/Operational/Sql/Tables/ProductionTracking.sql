-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.ProductionTracking
--  Layer:          Infrastructure -- SQL Scripts
--  File:           ProductionTracking.sql
--  Created:        2026-08-03
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the ProductionTracking module -- new-build backing
--                  v1/production-tracking/* (see docs/Phase5/ui-gaps-scoping.md
--                  "ProductionTracking" section, the largest/riskiest of the 9 UI gaps).
--                  [ProductionLines] is the genuinely-new minimal master-data table; the existing
--                  telemetry value objects (Observability.Telemetry) are recorded against a
--                  [ProductionRuns] row's MachineId rather than duplicating a second telemetry
--                  store. [ProductionDowntimeEvents] mirrors the real
--                  Observability.Telemetry.ValueObjects.DowntimeEvent shape so the bottleneck-
--                  detection read path can feed the real TelemetryAnomalyDetector directly.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Operational')
BEGIN
    EXEC('CREATE SCHEMA [Operational]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ProductionLines' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[ProductionLines] (
        [ProductionLineId]  UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER NULL,
        [LineCode]          NVARCHAR(50)     NOT NULL,
        [LineName]          NVARCHAR(200)    NOT NULL,
        [Status]            NVARCHAR(20)     NOT NULL,
        [CreatedAtUtc]      DATETIME2(3)     NOT NULL,
        [CreatedBy]         UNIQUEIDENTIFIER NOT NULL,
        [UpdatedAtUtc]      DATETIME2(3)     NULL,
        [IsDeleted]         BIT              NOT NULL CONSTRAINT DF_ProductionLines_IsDeleted DEFAULT (0),
        [RowVersion]        ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ProductionLines_TenantId] ON [Operational].[ProductionLines] ([TenantId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ProductionRuns' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[ProductionRuns] (
        [ProductionRunId]       UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER NULL,
        [Status]                NVARCHAR(20)     NOT NULL,
        [RunCode]               NVARCHAR(50)     NOT NULL,
        [LineId]                UNIQUEIDENTIFIER NOT NULL,
        [LineName]              NVARCHAR(200)    NOT NULL,
        [MachineId]             UNIQUEIDENTIFIER NOT NULL,
        [OperatorName]          NVARCHAR(200)    NULL,
        [IdealCycleTimeSeconds] FLOAT            NOT NULL,
        [GoodUnitCount]         INT              NOT NULL CONSTRAINT DF_ProductionRuns_GoodUnitCount DEFAULT (0),
        [ScrapUnitCount]        INT              NOT NULL CONSTRAINT DF_ProductionRuns_ScrapUnitCount DEFAULT (0),
        [DowntimeMinutesTotal]  FLOAT            NOT NULL CONSTRAINT DF_ProductionRuns_DowntimeMinutesTotal DEFAULT (0),
        [StartedAtUtc]          DATETIME2(3)     NOT NULL,
        [EndedAtUtc]            DATETIME2(3)     NULL,
        [CreatedAtUtc]          DATETIME2(3)     NOT NULL,
        [CreatedBy]             UNIQUEIDENTIFIER NOT NULL,
        [UpdatedAtUtc]          DATETIME2(3)     NULL,
        [IsDeleted]             BIT              NOT NULL CONSTRAINT DF_ProductionRuns_IsDeleted DEFAULT (0),
        [RowVersion]            ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ProductionRuns_TenantId_Status] ON [Operational].[ProductionRuns] ([TenantId], [Status]) INCLUDE ([MachineId], [StartedAtUtc]);
    CREATE NONCLUSTERED INDEX [IX_ProductionRuns_TenantId_StartedAtUtc] ON [Operational].[ProductionRuns] ([TenantId], [StartedAtUtc]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ProductionBatches' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[ProductionBatches] (
        [BatchId]           UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER NOT NULL,
        [ProductionRunId]   UNIQUEIDENTIFIER NOT NULL,
        [BatchCode]         NVARCHAR(50)     NOT NULL,
        [Status]            NVARCHAR(20)     NOT NULL,
        [StartedAtUtc]      DATETIME2(3)     NOT NULL,
        [CreatedAtUtc]      DATETIME2(3)     NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ProductionBatches_TenantId_RunId] ON [Operational].[ProductionBatches] ([TenantId], [ProductionRunId]);
    CREATE NONCLUSTERED INDEX [IX_ProductionBatches_TenantId_StartedAtUtc] ON [Operational].[ProductionBatches] ([TenantId], [StartedAtUtc]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ProductionRunEvents' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[ProductionRunEvents] (
        [EventId]           UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER NOT NULL,
        [ProductionRunId]   UNIQUEIDENTIFIER NOT NULL,
        [RunCode]           NVARCHAR(50)     NOT NULL,
        [RunEventType]      NVARCHAR(50)     NOT NULL,
        [Description]       NVARCHAR(2000)   NULL,
        [OccurredAtUtc]     DATETIME2(3)     NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ProductionRunEvents_TenantId_RunId] ON [Operational].[ProductionRunEvents] ([TenantId], [ProductionRunId]);
    CREATE NONCLUSTERED INDEX [IX_ProductionRunEvents_TenantId_OccurredAtUtc] ON [Operational].[ProductionRunEvents] ([TenantId], [OccurredAtUtc]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ProductionDowntimeEvents' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[ProductionDowntimeEvents] (
        [EventId]           UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER NOT NULL,
        [ProductionRunId]   UNIQUEIDENTIFIER NOT NULL,
        [MachineId]         UNIQUEIDENTIFIER NOT NULL,
        [ReasonCode]        NVARCHAR(50)     NOT NULL,
        [StartUtc]          DATETIME2(3)     NOT NULL,
        [EndUtc]            DATETIME2(3)     NULL,
        [IsPlanned]         BIT              NOT NULL CONSTRAINT DF_ProductionDowntimeEvents_IsPlanned DEFAULT (0)
    );

    CREATE NONCLUSTERED INDEX [IX_ProductionDowntimeEvents_TenantId_RunId] ON [Operational].[ProductionDowntimeEvents] ([TenantId], [ProductionRunId]);
END
GO
