-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.ExportItems
--  Layer:          Infrastructure - SQL Scripts
--  File:           OperationalExportItems.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the OperationalExportItems aggregate. This is a
--                  cross-cutting, denormalized export staging table that carries a
--                  flattened row per exportable Operational resource (Alerts,
--                  Checklists, Contractor, PPEManagement, RiskManagement, Schedules,
--                  Training, etc.), populated by the owning aggregates' write paths.
--                  It backs IExportOperationalRepository.GenerateExportAsync, which
--                  has no single natural source table of its own because it spans
--                  every Operational sub-aggregate.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'OperationalExportItems' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[OperationalExportItems] (
        [Id]                UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER    NULL,
        [ResourceType]      NVARCHAR(100)       NOT NULL,
        [ResourceId]        UNIQUEIDENTIFIER    NOT NULL,
        [Title]             NVARCHAR(300)       NOT NULL,
        [Status]            NVARCHAR(50)        NOT NULL,
        [EffectiveAtUtc]    DATETIME2(3)        NOT NULL,
        [OwnerId]           UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_OperationalExportItems_TenantId_EffectiveAtUtc]
        ON [Operational].[OperationalExportItems] ([TenantId], [EffectiveAtUtc])
        INCLUDE ([SiteId], [Status], [ResourceType]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[OperationalExportItems]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Operational].[OperationalExportItems] ADD RowVersion ROWVERSION NOT NULL;
END
GO
