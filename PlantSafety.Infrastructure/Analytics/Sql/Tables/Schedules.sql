-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Schedules
--  Layer:          Infrastructure - SQL Scripts
--  File:           Schedules.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Schedules read-model aggregate backing
--                  ISchedulesRepository.GetScheduleItemsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Schedules' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[Schedules] (
        [ScheduleItemId]    UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER    NULL,
        [ScheduleName]      NVARCHAR(200)       NOT NULL,
        [Frequency]         NVARCHAR(50)        NOT NULL,
        [NextRunAtUtc]      DATETIME2(3)        NOT NULL,
        [Status]            NVARCHAR(50)        NOT NULL,
        [CreatedAtUtc]      DATETIME2(3)        NOT NULL,
        [CreatedBy]         UNIQUEIDENTIFIER    NOT NULL,
        [RowVersion]        ROWVERSION          NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_Schedules_TenantId_NextRunAtUtc]
        ON [Analytics].[Schedules] ([TenantId], [NextRunAtUtc])
        INCLUDE ([SiteId], [Status], [ScheduleName]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Analytics].[Schedules]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Analytics].[Schedules] ADD RowVersion ROWVERSION NOT NULL;
END
GO
