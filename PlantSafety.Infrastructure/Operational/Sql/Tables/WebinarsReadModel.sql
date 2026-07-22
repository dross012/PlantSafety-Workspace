-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.Webinars
--  Layer:          Infrastructure - SQL Scripts
--  File:           WebinarsReadModel.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the WebinarsReadModel read-model aggregate backing
--                  IWebinarsRepository.GetWebinarsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'WebinarsReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[WebinarsReadModel] (
        [WebinarId]       UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [Title]                   NVARCHAR(400)       NOT NULL,
        [HostId]                  UNIQUEIDENTIFIER    NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [ScheduledAtUtc]  DATETIME2(3)        NOT NULL

    );

    CREATE NONCLUSTERED INDEX [IX_WebinarsReadModel_TenantId_ScheduledAtUtc]
        ON [Operational].[WebinarsReadModel] ([TenantId], [ScheduledAtUtc])
        INCLUDE ([SiteId]);
END
GO
