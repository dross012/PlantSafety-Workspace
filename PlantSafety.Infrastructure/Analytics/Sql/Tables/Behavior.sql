-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Culture.Behavior
--  Layer:          Infrastructure - SQL Scripts
--  File:           Behavior.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Behavior read-model aggregate backing
--                  IBehaviorRepository.GetBehaviorObservationsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Behavior' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[Behavior] (
        [ObservationId]     UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER    NULL,
        [EmployeeId]        UNIQUEIDENTIFIER    NOT NULL,
        [BehaviorCategory]  NVARCHAR(200)       NOT NULL,
        [ObservationType]   NVARCHAR(50)        NOT NULL,
        [Status]            NVARCHAR(50)        NOT NULL,
        [ObservedAtUtc]     DATETIME2(3)        NOT NULL,
        [ObservedBy]        UNIQUEIDENTIFIER    NOT NULL,
        [RowVersion]        ROWVERSION          NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_Behavior_TenantId_ObservedAtUtc]
        ON [Analytics].[Behavior] ([TenantId], [ObservedAtUtc])
        INCLUDE ([SiteId], [Status], [BehaviorCategory]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Analytics].[Behavior]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Analytics].[Behavior] ADD RowVersion ROWVERSION NOT NULL;
END
GO
