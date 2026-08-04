-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         DigitalTwin.EvacuationModeling.EvacuationRoutes
--  Layer:          Infrastructure - SQL Scripts
--  File:           EvacuationRoutes.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the EvacuationRoute aggregate root and its RouteObstructions
--                  child history table. TenantId is NOT NULL on both tables so they are
--                  automatically picked up by the live-discovery Row-Level-Security policy.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'DigitalTwin')
BEGIN
    EXEC('CREATE SCHEMA [DigitalTwin]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EvacuationRoutes' AND schema_id = SCHEMA_ID('DigitalTwin'))
BEGIN
    CREATE TABLE [DigitalTwin].[EvacuationRoutes] (
        [EvacuationRouteId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [RouteName] NVARCHAR(400) NOT NULL,
        [ZoneId] UNIQUEIDENTIFIER NOT NULL,
        [AssemblyPointId] UNIQUEIDENTIFIER NOT NULL,
        [PathGeometryRef] NVARCHAR(400) NOT NULL,
        [TwinModelVersion] NVARCHAR(100) NOT NULL,
        [EstimatedEgressTimeSeconds] INT NOT NULL,
        [ObstructionCount] INT NOT NULL CONSTRAINT DF_EvacuationRoutes_ObstructionCount DEFAULT (0),
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_EvacuationRoutes_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_EvacuationRoutes_TenantId_ZoneId]
        ON [DigitalTwin].[EvacuationRoutes] ([TenantId], [ZoneId])
        INCLUDE ([Status], [RouteName]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RouteObstructions' AND schema_id = SCHEMA_ID('DigitalTwin'))
BEGIN
    CREATE TABLE [DigitalTwin].[RouteObstructions] (
        [RouteObstructionId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [EvacuationRouteId] UNIQUEIDENTIFIER NOT NULL,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [Description] NVARCHAR(1000) NOT NULL,
        [Severity] NVARCHAR(50) NOT NULL,
        [RecordedAtUtc] DATETIME2(3) NOT NULL,
        [ClearedAtUtc] DATETIME2(3) NULL,
        CONSTRAINT FK_RouteObstructions_EvacuationRoutes FOREIGN KEY ([EvacuationRouteId])
            REFERENCES [DigitalTwin].[EvacuationRoutes] ([EvacuationRouteId])
    );

    CREATE NONCLUSTERED INDEX [IX_RouteObstructions_TenantId_Open]
        ON [DigitalTwin].[RouteObstructions] ([TenantId], [ClearedAtUtc])
        INCLUDE ([EvacuationRouteId], [Severity]);
END
GO
