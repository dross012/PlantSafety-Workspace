-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         DigitalTwin.EvacuationModeling.EvacuationBottlenecks
--  Layer:          Infrastructure - SQL Scripts
--  File:           EvacuationBottlenecks.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the EvacuationBottleneck aggregate root. Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'DigitalTwin')
BEGIN
    EXEC('CREATE SCHEMA [DigitalTwin]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EvacuationBottlenecks' AND schema_id = SCHEMA_ID('DigitalTwin'))
BEGIN
    CREATE TABLE [DigitalTwin].[EvacuationBottlenecks] (
        [EvacuationBottleneckId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [RouteId] UNIQUEIDENTIFIER NOT NULL,
        [SimulationRunId] UNIQUEIDENTIFIER NOT NULL,
        [BottleneckLocation] NVARCHAR(1000) NOT NULL,
        [DelaySeconds] INT NOT NULL,
        [RecurrenceCount] INT NOT NULL CONSTRAINT DF_EvacuationBottlenecks_RecurrenceCount DEFAULT (0),
        [DetectedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_EvacuationBottlenecks_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_EvacuationBottlenecks_TenantId_RouteId]
        ON [DigitalTwin].[EvacuationBottlenecks] ([TenantId], [RouteId])
        INCLUDE ([Status], [DetectedAtUtc]);
END
GO
