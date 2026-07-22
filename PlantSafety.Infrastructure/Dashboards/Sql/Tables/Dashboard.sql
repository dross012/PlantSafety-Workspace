-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Dashboards.Dashboard
--  Layer:          Infrastructure (SQL)
--  File:           Dashboard.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Canonical tables for the Dashboards.Dashboard aggregate (distinct from, and
--                  NOT to be confused with, the pre-existing [Analytics].[Dashboard] table owned
--                  by the Analytics.Dashboard "canonical reference module"). Schema [Dashboards].
--                  TenantId is UNIQUEIDENTIFIER NOT NULL on every table so the platform's
--                  Security.fn_TenantAccessPredicate Row-Level-Security policy auto-discovers and
--                  protects these tables the next time 90-row-level-security.sql is (re-)run.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Dashboards')
    EXEC('CREATE SCHEMA [Dashboards] AUTHORIZATION dbo;');
GO

-- ----------------------------------------------------------------------------
-- [Dashboards].[Dashboard] -- aggregate root table
-- ----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'Dashboards' AND t.name = 'Dashboard')
BEGIN
    CREATE TABLE [Dashboards].[Dashboard]
    (
        [DashboardId]   UNIQUEIDENTIFIER NOT NULL,
        [TenantId]      UNIQUEIDENTIFIER NOT NULL,
        [SiteId]        UNIQUEIDENTIFIER NULL,
        [Status]        NVARCHAR(20)     NOT NULL,
        [Name]          NVARCHAR(200)    NOT NULL,
        [Description]   NVARCHAR(2000)   NOT NULL CONSTRAINT DF_Dashboard_Description DEFAULT (N''),
        [CategoryCode]  NVARCHAR(50)     NOT NULL,
        [Role]          NVARCHAR(30)     NOT NULL,
        [CreatedAtUtc]  DATETIME2(3)     NOT NULL,
        [CreatedBy]     UNIQUEIDENTIFIER NOT NULL,
        [UpdatedAtUtc]  DATETIME2(3)     NULL,
        [IsDeleted]     BIT              NOT NULL CONSTRAINT DF_Dashboard_IsDeleted DEFAULT (0),
        [RowVersion]    ROWVERSION,

        CONSTRAINT PK_Dashboard PRIMARY KEY CLUSTERED ([DashboardId]),
        CONSTRAINT CK_Dashboard_Status CHECK ([Status] IN (N'Draft', N'Published', N'Archived'))
    );

    CREATE NONCLUSTERED INDEX IX_Dashboard_TenantId_Status ON [Dashboards].[Dashboard] ([TenantId], [Status]) WHERE [IsDeleted] = 0;
    CREATE NONCLUSTERED INDEX IX_Dashboard_TenantId_Role ON [Dashboards].[Dashboard] ([TenantId], [Role]) WHERE [IsDeleted] = 0;
END
GO

-- ----------------------------------------------------------------------------
-- [Dashboards].[DashboardDefaultFilter] -- child value-object collection
-- ----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'Dashboards' AND t.name = 'DashboardDefaultFilter')
BEGIN
    CREATE TABLE [Dashboards].[DashboardDefaultFilter]
    (
        [Id]          UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_DashboardDefaultFilter_Id DEFAULT (NEWID()),
        [DashboardId] UNIQUEIDENTIFIER NOT NULL,
        [TenantId]    UNIQUEIDENTIFIER NOT NULL,
        [FilterKey]   NVARCHAR(100)    NOT NULL,
        [FilterValue] NVARCHAR(500)    NOT NULL,

        CONSTRAINT PK_DashboardDefaultFilter PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT FK_DashboardDefaultFilter_Dashboard FOREIGN KEY ([DashboardId]) REFERENCES [Dashboards].[Dashboard] ([DashboardId])
    );

    CREATE NONCLUSTERED INDEX IX_DashboardDefaultFilter_DashboardId ON [Dashboards].[DashboardDefaultFilter] ([DashboardId]);
END
GO

-- ----------------------------------------------------------------------------
-- [Dashboards].[DashboardWidgetPlacement] -- child entity: widget layout
-- ----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'Dashboards' AND t.name = 'DashboardWidgetPlacement')
BEGIN
    CREATE TABLE [Dashboards].[DashboardWidgetPlacement]
    (
        [PlacementId] UNIQUEIDENTIFIER NOT NULL,
        [DashboardId] UNIQUEIDENTIFIER NOT NULL,
        [TenantId]    UNIQUEIDENTIFIER NOT NULL,
        [WidgetId]    UNIQUEIDENTIFIER NOT NULL,
        [Row]         INT              NOT NULL,
        [Column]      INT              NOT NULL,
        [RowSpan]     INT              NOT NULL CONSTRAINT DF_DashboardWidgetPlacement_RowSpan DEFAULT (1),
        [ColumnSpan]  INT              NOT NULL CONSTRAINT DF_DashboardWidgetPlacement_ColumnSpan DEFAULT (1),
        [CreatedAtUtc] DATETIME2(3)    NOT NULL CONSTRAINT DF_DashboardWidgetPlacement_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        [IsDeleted]   BIT              NOT NULL CONSTRAINT DF_DashboardWidgetPlacement_IsDeleted DEFAULT (0),

        CONSTRAINT PK_DashboardWidgetPlacement PRIMARY KEY CLUSTERED ([PlacementId]),
        CONSTRAINT FK_DashboardWidgetPlacement_Dashboard FOREIGN KEY ([DashboardId]) REFERENCES [Dashboards].[Dashboard] ([DashboardId])
    );

    CREATE NONCLUSTERED INDEX IX_DashboardWidgetPlacement_DashboardId ON [Dashboards].[DashboardWidgetPlacement] ([DashboardId]) WHERE [IsDeleted] = 0;
    CREATE NONCLUSTERED INDEX IX_DashboardWidgetPlacement_WidgetId ON [Dashboards].[DashboardWidgetPlacement] ([WidgetId]) WHERE [IsDeleted] = 0;
END
GO

-- ----------------------------------------------------------------------------
-- [Dashboards].[DashboardWidgetPlacementFilterOverride] -- grandchild value-object collection
-- ----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'Dashboards' AND t.name = 'DashboardWidgetPlacementFilterOverride')
BEGIN
    CREATE TABLE [Dashboards].[DashboardWidgetPlacementFilterOverride]
    (
        [Id]          UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_DashboardWidgetPlacementFilterOverride_Id DEFAULT (NEWID()),
        [PlacementId] UNIQUEIDENTIFIER NOT NULL,
        [TenantId]    UNIQUEIDENTIFIER NOT NULL,
        [FilterKey]   NVARCHAR(100)    NOT NULL,
        [FilterValue] NVARCHAR(500)    NOT NULL,

        CONSTRAINT PK_DashboardWidgetPlacementFilterOverride PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT FK_DashboardWidgetPlacementFilterOverride_Placement FOREIGN KEY ([PlacementId]) REFERENCES [Dashboards].[DashboardWidgetPlacement] ([PlacementId])
    );

    CREATE NONCLUSTERED INDEX IX_DashboardWidgetPlacementFilterOverride_PlacementId ON [Dashboards].[DashboardWidgetPlacementFilterOverride] ([PlacementId]);
END
GO
