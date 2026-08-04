-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         HazardousWasteManagement.WasteContainers
--  Layer:          Infrastructure - SQL Scripts
--  File:           WasteContainers.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.2
--  Description:    Table DDL for the WasteContainer aggregate root backing IWasteContainerRepository.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'HazardousWasteManagement')
BEGIN
    EXEC('CREATE SCHEMA [HazardousWasteManagement]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'WasteContainers' AND schema_id = SCHEMA_ID('HazardousWasteManagement'))
BEGIN
    CREATE TABLE [HazardousWasteManagement].[WasteContainers] (
        [WasteContainerId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [AccumulationAreaId] UNIQUEIDENTIFIER NOT NULL,
        [WasteStreamId] UNIQUEIDENTIFIER NOT NULL,
        [ContainerLabel] NVARCHAR(200) NOT NULL,
        [CapacityGallons] DECIMAL(12,2) NOT NULL,
        [RegulatoryTimeLimitDays] INT NOT NULL CONSTRAINT DF_WasteContainers_RegulatoryTimeLimitDays DEFAULT (90),
        [OpenedAtUtc] DATETIME2(3) NOT NULL,
        [ClosedAtUtc] DATETIME2(3) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_WasteContainers_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_WasteContainers_AccumulationAreaId]
        ON [HazardousWasteManagement].[WasteContainers] ([AccumulationAreaId])
        INCLUDE ([Status], [OpenedAtUtc], [RegulatoryTimeLimitDays]);
END
GO
