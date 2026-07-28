-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Reporting
--  Layer:          Infrastructure - SQL Scripts
--  File:           Reporting.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Reporting read-model aggregate backing
--                  IReportingRepository.GetReportItemsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Reporting' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[Reporting] (
        [ReportItemId]      UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER    NULL,
        [ReportName]        NVARCHAR(300)       NOT NULL,
        [ReportCategory]    NVARCHAR(100)       NOT NULL,
        [Status]            NVARCHAR(50)        NOT NULL,
        [GeneratedAtUtc]    DATETIME2(3)        NOT NULL,
        [GeneratedBy]       UNIQUEIDENTIFIER    NOT NULL,
        [RowVersion]        ROWVERSION          NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_Reporting_TenantId_GeneratedAtUtc]
        ON [Analytics].[Reporting] ([TenantId], [GeneratedAtUtc])
        INCLUDE ([SiteId], [Status], [ReportCategory]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Analytics].[Reporting]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Analytics].[Reporting] ADD RowVersion ROWVERSION NOT NULL;
END
GO
