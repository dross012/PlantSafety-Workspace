-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         DigitalTwin.EvacuationModeling.EvacuationZones
--  Layer:          Infrastructure - SQL Scripts
--  File:           EvacuationZones.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the EvacuationZone aggregate root and its OccupancyReadings
--                  child history table. Occupancy data is aggregate/anonymized per module
--                  data-governance rules -- no individually identifiable occupant data is stored.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'DigitalTwin')
BEGIN
    EXEC('CREATE SCHEMA [DigitalTwin]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EvacuationZones' AND schema_id = SCHEMA_ID('DigitalTwin'))
BEGIN
    CREATE TABLE [DigitalTwin].[EvacuationZones] (
        [EvacuationZoneId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [ZoneName] NVARCHAR(400) NOT NULL,
        [TwinModelVersion] NVARCHAR(100) NOT NULL,
        [ZoneCapacity] INT NOT NULL,
        [OccupancyThreshold] INT NOT NULL,
        [CurrentOccupancyCount] INT NOT NULL CONSTRAINT DF_EvacuationZones_CurrentOccupancyCount DEFAULT (0),
        [LastOccupancyReadingAtUtc] DATETIME2(3) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_EvacuationZones_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_EvacuationZones_TenantId_SiteId]
        ON [DigitalTwin].[EvacuationZones] ([TenantId], [SiteId])
        INCLUDE ([Status], [ZoneName]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'OccupancyReadings' AND schema_id = SCHEMA_ID('DigitalTwin'))
BEGIN
    CREATE TABLE [DigitalTwin].[OccupancyReadings] (
        [OccupancyReadingId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [EvacuationZoneId] UNIQUEIDENTIFIER NOT NULL,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [HeadcountEstimate] INT NOT NULL,
        [Source] NVARCHAR(50) NOT NULL,
        [RecordedAtUtc] DATETIME2(3) NOT NULL,
        [ExceededThreshold] BIT NOT NULL,
        CONSTRAINT FK_OccupancyReadings_EvacuationZones FOREIGN KEY ([EvacuationZoneId])
            REFERENCES [DigitalTwin].[EvacuationZones] ([EvacuationZoneId])
    );

    CREATE NONCLUSTERED INDEX [IX_OccupancyReadings_ZoneId_RecordedAtUtc]
        ON [DigitalTwin].[OccupancyReadings] ([EvacuationZoneId], [RecordedAtUtc]);
END
GO
