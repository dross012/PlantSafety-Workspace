-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         AutonomousVehicleSafety.AutonomousVehicleAsset
--  Layer:          Infrastructure - SQL Scripts
--  File:           AutonomousVehicleAsset.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the AutonomousVehicleAsset aggregate root backing
--                  IAutonomousVehicleAssetRepository. TenantId is NOT NULL so this table is
--                  automatically picked up by the live-discovery Row-Level-Security policy in
--                  90-row-level-security.sql (Security.fn_TenantAccessPredicate).
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'AutonomousVehicleSafety')
BEGIN
    EXEC('CREATE SCHEMA [AutonomousVehicleSafety]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AutonomousVehicleAsset' AND schema_id = SCHEMA_ID('AutonomousVehicleSafety'))
BEGIN
    CREATE TABLE [AutonomousVehicleSafety].[AutonomousVehicleAsset] (
        [AutonomousVehicleAssetId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [AssetTag] NVARCHAR(200) NOT NULL,
        [VehicleType] NVARCHAR(100) NOT NULL,
        [AutonomyLevel] NVARCHAR(100) NOT NULL,
        [CurrentAutonomyMode] NVARCHAR(100) NOT NULL,
        [SafetyCaseValidationId] UNIQUEIDENTIFIER NULL,
        [ModeTransitionCount] INT NOT NULL CONSTRAINT DF_AutonomousVehicleAsset_ModeTransitionCount DEFAULT (0),
        [SuspensionReason] NVARCHAR(1000) NULL,
        [SuspendedAtUtc] DATETIME2(3) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_AutonomousVehicleAsset_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_AutonomousVehicleAsset_TenantId_SiteId]
        ON [AutonomousVehicleSafety].[AutonomousVehicleAsset] ([TenantId], [SiteId])
        INCLUDE ([Status], [AssetTag]);
END
GO
