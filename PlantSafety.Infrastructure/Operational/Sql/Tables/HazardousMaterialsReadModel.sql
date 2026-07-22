-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.HazardousMaterials
--  Layer:          Infrastructure - SQL Scripts
--  File:           HazardousMaterialsReadModel.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the HazardousMaterialsReadModel read-model aggregate backing
--                  IHazardousMaterialsRepository.GetHazardousMaterialsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'HazardousMaterialsReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[HazardousMaterialsReadModel] (
        [HazardousMaterialId] UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [MaterialName]            NVARCHAR(400)       NOT NULL,
        [HazardClass]             NVARCHAR(400)       NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [RegisteredAtUtc] DATETIME2(3)        NOT NULL,
        [RegisteredBy]    UNIQUEIDENTIFIER    NULL
    );

    CREATE NONCLUSTERED INDEX [IX_HazardousMaterialsReadModel_TenantId_RegisteredAtUtc]
        ON [Operational].[HazardousMaterialsReadModel] ([TenantId], [RegisteredAtUtc])
        INCLUDE ([SiteId]);
END
GO
