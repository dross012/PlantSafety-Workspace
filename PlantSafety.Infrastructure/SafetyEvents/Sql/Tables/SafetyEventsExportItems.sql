-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.SafetyEventsExportItems
--  Layer:          Infrastructure – SQL Scripts
--  File:           SafetyEventsExportItems.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the SafetyEventsExportItems aggregate. This is a
--                  cross-cutting, denormalized export staging table that carries a
--                  flattened row per exportable SafetyEvents resource (Incident,
--                  NearMiss, Observation, Drill, Emergency, Crisis, RootCauseAnalysis),
--                  populated by the owning aggregates' write paths. It backs
--                  IExportSafetyEventsRepository.GenerateExportAsync, which has no
--                  single natural source table of its own because it spans every
--                  SafetyEvents sub-aggregate.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SafetyEventsExportItems' AND schema_id = SCHEMA_ID('SafetyEvents'))
BEGIN
    CREATE TABLE [SafetyEvents].[SafetyEventsExportItems] (
        [Id]             UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]       UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]         UNIQUEIDENTIFIER    NULL,
        [ResourceType]   NVARCHAR(100)       NOT NULL,
        [ResourceId]     UNIQUEIDENTIFIER    NOT NULL,
        [Title]          NVARCHAR(300)       NOT NULL,
        [Status]         NVARCHAR(50)        NOT NULL,
        [Severity]       NVARCHAR(50)        NULL,
        [EffectiveAtUtc] DATETIME2(3)        NOT NULL,
        [OwnerId]        UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_SafetyEventsExportItems_TenantId_EffectiveAtUtc]
        ON [SafetyEvents].[SafetyEventsExportItems] ([TenantId], [EffectiveAtUtc])
        INCLUDE ([SiteId], [Status], [Severity], [ResourceType]);
END
GO
