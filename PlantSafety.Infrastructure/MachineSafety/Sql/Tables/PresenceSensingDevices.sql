-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         MachineSafety.PresenceSensingDevices
--  Layer:          Infrastructure - SQL Scripts
--  File:           PresenceSensingDevices.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the PresenceSensingDevices aggregate root backing IPresenceSensingDevicesRepository.
--                  TenantId is NOT NULL so this table is automatically picked up by the
--                  live-discovery Row-Level-Security policy in 90-row-level-security.sql
--                  (Security.fn_TenantAccessPredicate).
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'MachineSafety')
BEGIN
    EXEC('CREATE SCHEMA [MachineSafety]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PresenceSensingDevices' AND schema_id = SCHEMA_ID('MachineSafety'))
BEGIN
    CREATE TABLE [MachineSafety].[PresenceSensingDevices] (
        [PresenceSensingDevicesId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [DeviceTitle] NVARCHAR(400) NOT NULL,
        [DeviceType] NVARCHAR(100) NOT NULL,
        [EquipmentLocation] NVARCHAR(400) NOT NULL,
        [CalibratedByUserId] UNIQUEIDENTIFIER NOT NULL,
        [CalibratedAtUtc] DATETIME2(3) NULL,
        [NextCalibrationDueAtUtc] DATETIME2(3) NULL,
        [TriggerEventCount] INT NOT NULL CONSTRAINT DF_PresenceSensingDevices_TriggerEventCount DEFAULT (0),
        [DetectionAccuracyScore] DECIMAL(9,2) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_PresenceSensingDevices_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PresenceSensingDevices_TenantId_SiteId]
        ON [MachineSafety].[PresenceSensingDevices] ([TenantId], [SiteId])
        INCLUDE ([Status], [DeviceTitle]);
END
GO
