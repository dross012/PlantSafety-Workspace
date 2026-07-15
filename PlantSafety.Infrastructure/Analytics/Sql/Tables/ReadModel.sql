-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Dashboard.ReadModel
--  Layer:          Infrastructure - SQL Scripts
--  File:           ReadModel.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the ReadModel read-model aggregate backing
--                  IReadModelRepository.GetDashboardRowsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ReadModel' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[ReadModel] (
        [RowId]             UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER    NULL,
        [WidgetName]        NVARCHAR(200)       NOT NULL,
        [WidgetValue]       NVARCHAR(400)       NOT NULL,
        [DisplayOrder]      INT                 NOT NULL,
        [Status]            NVARCHAR(50)        NOT NULL,
        [RefreshedAtUtc]    DATETIME2(3)        NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ReadModel_TenantId_RefreshedAtUtc]
        ON [Analytics].[ReadModel] ([TenantId], [RefreshedAtUtc])
        INCLUDE ([SiteId], [Status], [WidgetName], [DisplayOrder]);
END
GO
