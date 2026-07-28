-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.Inspections
--  Layer:          Infrastructure - SQL Scripts
--  File:           InspectionsReadModel.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the InspectionsReadModel read-model aggregate backing
--                  IInspectionsRepository.GetInspectionsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'InspectionsReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[InspectionsReadModel] (
        [InspectionId]    UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [InspectionType]          NVARCHAR(400)       NOT NULL,
        [Outcome]                 NVARCHAR(400)       NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [InspectedAtUtc]  DATETIME2(3)        NOT NULL,
        [InspectedBy]     UNIQUEIDENTIFIER    NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_InspectionsReadModel_TenantId_InspectedAtUtc]
        ON [Operational].[InspectionsReadModel] ([TenantId], [InspectedAtUtc])
        INCLUDE ([SiteId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[InspectionsReadModel]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Operational].[InspectionsReadModel] ADD RowVersion ROWVERSION NOT NULL;
END
GO
