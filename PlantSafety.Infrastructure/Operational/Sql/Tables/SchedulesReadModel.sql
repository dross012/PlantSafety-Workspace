-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.Schedules
--  Layer:          Infrastructure - SQL Scripts
--  File:           SchedulesReadModel.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the SchedulesReadModel read-model aggregate backing
--                  ISchedulesRepository.GetSchedulesAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SchedulesReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[SchedulesReadModel] (
        [ScheduleId]      UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [ShiftName]               NVARCHAR(400)       NOT NULL,
        [ScheduleType]            NVARCHAR(400)       NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [ScheduledAtUtc]  DATETIME2(3)        NOT NULL,
        [AssignedBy]      UNIQUEIDENTIFIER    NULL
    );

    CREATE NONCLUSTERED INDEX [IX_SchedulesReadModel_TenantId_ScheduledAtUtc]
        ON [Operational].[SchedulesReadModel] ([TenantId], [ScheduledAtUtc])
        INCLUDE ([SiteId]);
END
GO
