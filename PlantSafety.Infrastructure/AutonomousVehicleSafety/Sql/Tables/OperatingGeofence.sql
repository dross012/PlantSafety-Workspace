-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         AutonomousVehicleSafety.OperatingGeofence
--  Layer:          Infrastructure - SQL Scripts
--  File:           OperatingGeofence.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the OperatingGeofence aggregate root backing
--                  IOperatingGeofenceRepository. TenantId is NOT NULL so this table is
--                  automatically picked up by the live-discovery Row-Level-Security policy in
--                  90-row-level-security.sql (Security.fn_TenantAccessPredicate).
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'AutonomousVehicleSafety')
BEGIN
    EXEC('CREATE SCHEMA [AutonomousVehicleSafety]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'OperatingGeofence' AND schema_id = SCHEMA_ID('AutonomousVehicleSafety'))
BEGIN
    CREATE TABLE [AutonomousVehicleSafety].[OperatingGeofence] (
        [OperatingGeofenceId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [GeofenceName] NVARCHAR(200) NOT NULL,
        [ZoneType] NVARCHAR(100) NOT NULL,
        [BoundaryDefinition] NVARCHAR(MAX) NOT NULL,
        [MaxSpeedKph] DECIMAL(9,2) NULL,
        [ViolationCount] INT NOT NULL CONSTRAINT DF_OperatingGeofence_ViolationCount DEFAULT (0),
        [LastAuditedAtUtc] DATETIME2(3) NULL,
        [NextAuditDueAtUtc] DATETIME2(3) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_OperatingGeofence_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_OperatingGeofence_TenantId_SiteId]
        ON [AutonomousVehicleSafety].[OperatingGeofence] ([TenantId], [SiteId])
        INCLUDE ([Status], [GeofenceName], [ViolationCount]);
END
GO
