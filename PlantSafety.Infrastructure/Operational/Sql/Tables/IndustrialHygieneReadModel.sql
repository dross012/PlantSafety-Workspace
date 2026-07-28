-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.IndustrialHygiene
--  Layer:          Infrastructure - SQL Scripts
--  File:           IndustrialHygieneReadModel.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the IndustrialHygieneReadModel read-model aggregate backing
--                  IIndustrialHygieneRepository.GetIndustrialHygieneAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'IndustrialHygieneReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[IndustrialHygieneReadModel] (
        [IndustrialHygieneSampleId] UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [SampleType]              NVARCHAR(400)       NOT NULL,
        [ExposureLevel]           NVARCHAR(400)       NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [SampledAtUtc]    DATETIME2(3)        NOT NULL,
        [SampledBy]       UNIQUEIDENTIFIER    NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_IndustrialHygieneReadModel_TenantId_SampledAtUtc]
        ON [Operational].[IndustrialHygieneReadModel] ([TenantId], [SampledAtUtc])
        INCLUDE ([SiteId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[IndustrialHygieneReadModel]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Operational].[IndustrialHygieneReadModel] ADD RowVersion ROWVERSION NOT NULL;
END
GO
