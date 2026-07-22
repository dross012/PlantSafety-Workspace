-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         MachineSafety.EmergencyStopDevices
--  Layer:          Infrastructure - SQL Scripts
--  File:           EmergencyStopDevices.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the EmergencyStopDevices aggregate root backing IEmergencyStopDevicesRepository.
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

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EmergencyStopDevices' AND schema_id = SCHEMA_ID('MachineSafety'))
BEGIN
    CREATE TABLE [MachineSafety].[EmergencyStopDevices] (
        [EmergencyStopDevicesId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [DeviceTitle] NVARCHAR(400) NOT NULL,
        [DeviceType] NVARCHAR(100) NOT NULL,
        [EquipmentLocation] NVARCHAR(400) NOT NULL,
        [TestedByUserId] UNIQUEIDENTIFIER NOT NULL,
        [TestedAtUtc] DATETIME2(3) NULL,
        [NextTestDueAtUtc] DATETIME2(3) NULL,
        [FailureCount] INT NOT NULL CONSTRAINT DF_EmergencyStopDevices_FailureCount DEFAULT (0),
        [ResponseTimeScore] DECIMAL(9,2) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_EmergencyStopDevices_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_EmergencyStopDevices_TenantId_SiteId]
        ON [MachineSafety].[EmergencyStopDevices] ([TenantId], [SiteId])
        INCLUDE ([Status], [DeviceTitle]);
END
GO
