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

-- Additive columns backing the report-catalog detail surface (ReportDetailDto) used by the
-- PlantSafety.UI Reporting module's Report Detail page. Nullable so existing rows remain valid.
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Analytics].[Reporting]') AND name = 'Description')
BEGIN
    ALTER TABLE [Analytics].[Reporting] ADD Description NVARCHAR(1000) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Analytics].[Reporting]') AND name = 'PeriodStartUtc')
BEGIN
    ALTER TABLE [Analytics].[Reporting] ADD PeriodStartUtc DATE NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Analytics].[Reporting]') AND name = 'PeriodEndUtc')
BEGIN
    ALTER TABLE [Analytics].[Reporting] ADD PeriodEndUtc DATE NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Analytics].[Reporting]') AND name = 'AvailableFormats')
BEGIN
    ALTER TABLE [Analytics].[Reporting] ADD AvailableFormats NVARCHAR(200) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Analytics].[Reporting]') AND name = 'SourceModule')
BEGIN
    ALTER TABLE [Analytics].[Reporting] ADD SourceModule NVARCHAR(100) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Analytics].[Reporting]') AND name = 'OwnerName')
BEGIN
    ALTER TABLE [Analytics].[Reporting] ADD OwnerName NVARCHAR(200) NULL;
END
GO
