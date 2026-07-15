-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.Observation
--  Layer:          Infrastructure – SQL Scripts
--  File:           ObservationItems.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Observation read-model, backing
--                  IObservationRepository.GetObservationsAsync. Named
--                  ObservationItems (not Observation) to avoid colliding with the
--                  unrelated [SafetyEvents].[usp_Observation_*] procedures already
--                  referenced by the separate Features-tree ObservationRepository
--                  in this same module.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ObservationItems' AND schema_id = SCHEMA_ID('SafetyEvents'))
BEGIN
    CREATE TABLE [SafetyEvents].[ObservationItems] (
        ObservationId   UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId        UNIQUEIDENTIFIER NOT NULL,
        SiteId          UNIQUEIDENTIFIER NULL,
        ObservationType NVARCHAR(50)     NOT NULL,
        Category        NVARCHAR(50)     NOT NULL,
        Status          NVARCHAR(50)     NOT NULL,
        ObservedAtUtc   DATETIME2(3)     NOT NULL,
        ObservedBy      UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ObservationItems_TenantId_SiteId] ON [SafetyEvents].[ObservationItems] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_ObservationItems_ObservedAtUtc] ON [SafetyEvents].[ObservationItems] (ObservedAtUtc);
END
GO
