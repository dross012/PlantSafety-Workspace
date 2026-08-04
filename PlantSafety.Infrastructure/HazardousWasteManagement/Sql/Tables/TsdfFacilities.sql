-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         HazardousWasteManagement.TsdfFacilities
--  Layer:          Infrastructure - SQL Scripts
--  File:           TsdfFacilities.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.2
--  Description:    Table DDL for the TsdfFacility aggregate root backing ITsdfFacilityRepository.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'HazardousWasteManagement')
BEGIN
    EXEC('CREATE SCHEMA [HazardousWasteManagement]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TsdfFacilities' AND schema_id = SCHEMA_ID('HazardousWasteManagement'))
BEGIN
    CREATE TABLE [HazardousWasteManagement].[TsdfFacilities] (
        [TsdfFacilityId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [FacilityName] NVARCHAR(400) NOT NULL,
        [EpaFacilityId] NVARCHAR(50) NOT NULL,
        [PermitExpiresAtUtc] DATETIME2(3) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_TsdfFacilities_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_TsdfFacilities_TenantId]
        ON [HazardousWasteManagement].[TsdfFacilities] ([TenantId])
        INCLUDE ([Status]);
END
GO
