-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.AnalyticsExportItems
--  Layer:          Infrastructure - SQL Scripts
--  File:           AnalyticsExportItems.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the AnalyticsExportItems aggregate. This is a
--                  cross-cutting, denormalized export staging table that carries a
--                  flattened row per exportable Analytics resource (Culture,
--                  Dashboard, Predictive, Reporting, ScoreCards, etc.), populated
--                  by the owning aggregates' write paths. It backs
--                  IExportAnalyticsRepository.GenerateExportAsync, which has no
--                  single natural source table of its own because it spans every
--                  Analytics sub-aggregate. Mirrors the Compliance module's
--                  [Compliance].[ComplianceExportItems] pattern.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AnalyticsExportItems' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[AnalyticsExportItems] (
        [Id]                UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER    NULL,
        [ResourceType]      NVARCHAR(100)       NOT NULL,
        [ResourceId]        UNIQUEIDENTIFIER    NOT NULL,
        [Title]             NVARCHAR(300)       NOT NULL,
        [Status]            NVARCHAR(50)        NOT NULL,
        [EffectiveAtUtc]    DATETIME2(3)        NOT NULL,
        [OwnerId]           UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_AnalyticsExportItems_TenantId_EffectiveAtUtc]
        ON [Analytics].[AnalyticsExportItems] ([TenantId], [EffectiveAtUtc])
        INCLUDE ([SiteId], [Status], [ResourceType]);
END
GO
