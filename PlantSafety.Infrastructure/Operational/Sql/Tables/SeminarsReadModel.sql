-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.Seminars
--  Layer:          Infrastructure - SQL Scripts
--  File:           SeminarsReadModel.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the SeminarsReadModel read-model aggregate backing
--                  ISeminarsRepository.GetSeminarsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SeminarsReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[SeminarsReadModel] (
        [SeminarId]       UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [Title]                   NVARCHAR(400)       NOT NULL,
        [PresenterId]             UNIQUEIDENTIFIER    NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [ScheduledAtUtc]  DATETIME2(3)        NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_SeminarsReadModel_TenantId_ScheduledAtUtc]
        ON [Operational].[SeminarsReadModel] ([TenantId], [ScheduledAtUtc])
        INCLUDE ([SiteId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[SeminarsReadModel]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Operational].[SeminarsReadModel] ADD RowVersion ROWVERSION NOT NULL;
END
GO
