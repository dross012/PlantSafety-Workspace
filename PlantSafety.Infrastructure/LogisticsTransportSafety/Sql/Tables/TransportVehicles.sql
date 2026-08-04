-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         LogisticsTransportSafety.TransportVehicles
--  Layer:          Infrastructure - SQL Scripts
--  File:           TransportVehicles.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team (Claude Code sql-database-skill agent)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the TransportVehicle aggregate root backing
--                  ITransportVehicleRepository (Domain write repository) and the
--                  Application-layer read-model repository of the same simple name. Column list is
--                  column-for-column with TransportVehicleDapperRepository.TransportVehicleRow.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'LogisticsTransportSafety')
BEGIN
    EXEC('CREATE SCHEMA [LogisticsTransportSafety]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TransportVehicles' AND schema_id = SCHEMA_ID('LogisticsTransportSafety'))
BEGIN
    CREATE TABLE [LogisticsTransportSafety].[TransportVehicles] (
        [TransportVehicleId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(50) NOT NULL,
        [VehicleTitle] NVARCHAR(200) NOT NULL,
        [VehicleType] NVARCHAR(50) NOT NULL,
        [VehicleIdentifier] NVARCHAR(100) NOT NULL,
        [LastInspectionAtUtc] DATETIME2(3) NULL,
        [NextInspectionDueAtUtc] DATETIME2(3) NULL,
        [LastInspectionScore] DECIMAL(5,2) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_TransportVehicles_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_TransportVehicles_TenantId_SiteId]
        ON [LogisticsTransportSafety].[TransportVehicles] ([TenantId], [SiteId])
        INCLUDE ([Status], [VehicleType], [VehicleIdentifier]);

    CREATE NONCLUSTERED INDEX [IX_TransportVehicles_TenantId_NextInspectionDueAtUtc]
        ON [LogisticsTransportSafety].[TransportVehicles] ([TenantId], [NextInspectionDueAtUtc])
        INCLUDE ([Status], [SiteId]);
END
GO
