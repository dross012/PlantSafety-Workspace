-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.DeadlinesEPA
--  Layer:          Infrastructure - SQL Scripts
--  File:           DeadlinesEPA.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the DeadlinesEPA read-model aggregate backing
--                  IEPARepository.GetDeadlinesAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DeadlinesEPA' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[DeadlinesEPA] (
        [DeadlineId]              UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [EPARegulation]           NVARCHAR(300)       NOT NULL,
        [DueAtUtc]                DATETIME2(3)        NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [ResponsibleParty]        UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_DeadlinesEPA_TenantId_DueAtUtc]
        ON [Compliance].[DeadlinesEPA] ([TenantId], [DueAtUtc])
        INCLUDE ([SiteId]);
END
GO