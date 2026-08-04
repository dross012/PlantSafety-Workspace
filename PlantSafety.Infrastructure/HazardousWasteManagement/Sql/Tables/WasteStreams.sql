-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         HazardousWasteManagement.WasteStreams
--  Layer:          Infrastructure - SQL Scripts
--  File:           WasteStreams.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.2
--  Description:    Table DDL for the WasteStream aggregate root backing IWasteStreamRepository.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'HazardousWasteManagement')
BEGIN
    EXEC('CREATE SCHEMA [HazardousWasteManagement]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'WasteStreams' AND schema_id = SCHEMA_ID('HazardousWasteManagement'))
BEGIN
    CREATE TABLE [HazardousWasteManagement].[WasteStreams] (
        [WasteStreamId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [StreamName] NVARCHAR(400) NOT NULL,
        [EpaWasteCode] NVARCHAR(20) NOT NULL,
        [HazardClass] NVARCHAR(100) NOT NULL,
        [GeneratorEpaId] NVARCHAR(50) NOT NULL,
        [CharacterizationVersion] INT NOT NULL CONSTRAINT DF_WasteStreams_CharacterizationVersion DEFAULT (1),
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_WasteStreams_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_WasteStreams_TenantId_SiteId]
        ON [HazardousWasteManagement].[WasteStreams] ([TenantId], [SiteId])
        INCLUDE ([Status], [StreamName]);
END
GO
