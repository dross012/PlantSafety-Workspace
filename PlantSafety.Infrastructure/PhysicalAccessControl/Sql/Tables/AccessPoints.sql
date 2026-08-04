-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PhysicalAccessControl.AccessPoints
--  Layer:          Infrastructure - SQL Scripts
--  File:           AccessPoints.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the AccessPoint aggregate root backing IAccessPointRepository.
--                  TenantId is NOT NULL so this table is automatically picked up by the
--                  live-discovery Row-Level-Security policy in 90-row-level-security.sql
--                  (Security.fn_TenantAccessPredicate).
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'PhysicalAccessControl')
BEGIN
    EXEC('CREATE SCHEMA [PhysicalAccessControl]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AccessPoints' AND schema_id = SCHEMA_ID('PhysicalAccessControl'))
BEGIN
    CREATE TABLE [PhysicalAccessControl].[AccessPoints] (
        [AccessPointId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [AccessPointCode] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(2000) NOT NULL,
        [DeviceType] NVARCHAR(100) NOT NULL,
        [SafetyZoneId] UNIQUEIDENTIFIER NULL,
        [IsPerimeter] BIT NOT NULL CONSTRAINT DF_AccessPoints_IsPerimeter DEFAULT (0),
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_AccessPoints_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_AccessPoints_TenantId_SiteId]
        ON [PhysicalAccessControl].[AccessPoints] ([TenantId], [SiteId])
        INCLUDE ([Status], [AccessPointCode], [SafetyZoneId]);
END
GO
