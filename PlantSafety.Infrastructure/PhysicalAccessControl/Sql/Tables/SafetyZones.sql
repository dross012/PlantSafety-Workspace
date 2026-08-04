-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PhysicalAccessControl.SafetyZones
--  Layer:          Infrastructure - SQL Scripts
--  File:           SafetyZones.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the SafetyZone aggregate root backing ISafetyZoneRepository.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SafetyZones' AND schema_id = SCHEMA_ID('PhysicalAccessControl'))
BEGIN
    CREATE TABLE [PhysicalAccessControl].[SafetyZones] (
        [SafetyZoneId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [ZoneCode] NVARCHAR(100) NOT NULL,
        [ZoneName] NVARCHAR(200) NOT NULL,
        [HazardLevel] NVARCHAR(50) NOT NULL,
        [MaxOccupancy] INT NOT NULL CONSTRAINT DF_SafetyZones_MaxOccupancy DEFAULT (0),
        [CurrentOccupancy] INT NOT NULL CONSTRAINT DF_SafetyZones_CurrentOccupancy DEFAULT (0),
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_SafetyZones_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_SafetyZones_TenantId_SiteId]
        ON [PhysicalAccessControl].[SafetyZones] ([TenantId], [SiteId])
        INCLUDE ([Status], [ZoneCode]);
END
GO
