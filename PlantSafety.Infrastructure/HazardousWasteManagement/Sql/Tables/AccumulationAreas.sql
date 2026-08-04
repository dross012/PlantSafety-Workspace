-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         HazardousWasteManagement.AccumulationAreas
--  Layer:          Infrastructure - SQL Scripts
--  File:           AccumulationAreas.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.2
--  Description:    Table DDL for the AccumulationArea aggregate root backing IAccumulationAreaRepository.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'HazardousWasteManagement')
BEGIN
    EXEC('CREATE SCHEMA [HazardousWasteManagement]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AccumulationAreas' AND schema_id = SCHEMA_ID('HazardousWasteManagement'))
BEGIN
    CREATE TABLE [HazardousWasteManagement].[AccumulationAreas] (
        [AccumulationAreaId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [AreaName] NVARCHAR(400) NOT NULL,
        [LocationDescription] NVARCHAR(1000) NOT NULL,
        [TimeLimitDays] INT NOT NULL CONSTRAINT DF_AccumulationAreas_TimeLimitDays DEFAULT (90),
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_AccumulationAreas_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_AccumulationAreas_TenantId_SiteId]
        ON [HazardousWasteManagement].[AccumulationAreas] ([TenantId], [SiteId])
        INCLUDE ([Status]);
END
GO
