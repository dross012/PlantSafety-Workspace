-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.ComplianceExportItems
--  Layer:          Infrastructure - SQL Scripts
--  File:           ComplianceExportItems.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the ComplianceExportItems aggregate. This is a
--                  cross-cutting, denormalized export staging table that carries a
--                  flattened row per exportable Compliance resource (Policy,
--                  Procedure, RegulatoryCalendar deadline/inspection/renewal, etc.),
--                  populated by the owning aggregates' write paths. It backs
--                  IExportComplianceRepository.GenerateExportAsync, which has no
--                  single natural source table of its own because it spans every
--                  Compliance sub-aggregate.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ComplianceExportItems' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[ComplianceExportItems] (
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

    CREATE NONCLUSTERED INDEX [IX_ComplianceExportItems_TenantId_EffectiveAtUtc]
        ON [Compliance].[ComplianceExportItems] ([TenantId], [EffectiveAtUtc])
        INCLUDE ([SiteId], [Status], [ResourceType]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Compliance].[ComplianceExportItems]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Compliance].[ComplianceExportItems] ADD RowVersion ROWVERSION NOT NULL;
END
GO
