-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Dashboards.Widget
--  Layer:          Infrastructure (SQL)
--  File:           Widget.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Canonical tables for the Dashboards.Widget aggregate. Schema [Dashboards]
--                  (shared with Dashboards.Dashboard -- same bounded context). TenantId is
--                  UNIQUEIDENTIFIER NOT NULL on every table so the platform's
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
-- [Dashboards].[Widget] -- aggregate root table
-- ----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'Dashboards' AND t.name = 'Widget')
BEGIN
    CREATE TABLE [Dashboards].[Widget]
    (
        [WidgetId]                UNIQUEIDENTIFIER NOT NULL,
        [TenantId]                UNIQUEIDENTIFIER NOT NULL,
        [Status]                  NVARCHAR(20)     NOT NULL,
        [Name]                    NVARCHAR(200)    NOT NULL,
        [TypeCode]                NVARCHAR(30)      NOT NULL,
        [DataSourceModule]        NVARCHAR(100)    NOT NULL,
        [DataSourceQueryOrEndpoint] NVARCHAR(200)  NOT NULL,
        [RefreshIntervalSeconds]  INT              NOT NULL,
        [DeprecationReason]       NVARCHAR(500)    NULL,
        [CreatedAtUtc]            DATETIME2(3)     NOT NULL,
        [CreatedBy]               UNIQUEIDENTIFIER NOT NULL,
        [UpdatedAtUtc]            DATETIME2(3)     NULL,
        [IsDeleted]               BIT              NOT NULL CONSTRAINT DF_Widget_IsDeleted DEFAULT (0),
        [RowVersion]              ROWVERSION,

        CONSTRAINT PK_Widget PRIMARY KEY CLUSTERED ([WidgetId]),
        CONSTRAINT CK_Widget_Status CHECK ([Status] IN (N'Draft', N'Active', N'Deprecated', N'Retired'))
    );

    CREATE NONCLUSTERED INDEX IX_Widget_TenantId_Status ON [Dashboards].[Widget] ([TenantId], [Status]) WHERE [IsDeleted] = 0;
END
GO

-- ----------------------------------------------------------------------------
-- [Dashboards].[WidgetParameter] -- child value-object collection
-- ----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'Dashboards' AND t.name = 'WidgetParameter')
BEGIN
    CREATE TABLE [Dashboards].[WidgetParameter]
    (
        [Id]         UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_WidgetParameter_Id DEFAULT (NEWID()),
        [WidgetId]   UNIQUEIDENTIFIER NOT NULL,
        [TenantId]   UNIQUEIDENTIFIER NOT NULL,
        [ParamKey]   NVARCHAR(100)    NOT NULL,
        [ParamValue] NVARCHAR(500)    NOT NULL,

        CONSTRAINT PK_WidgetParameter PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT FK_WidgetParameter_Widget FOREIGN KEY ([WidgetId]) REFERENCES [Dashboards].[Widget] ([WidgetId])
    );

    CREATE NONCLUSTERED INDEX IX_WidgetParameter_WidgetId ON [Dashboards].[WidgetParameter] ([WidgetId]);
END
GO

-- ----------------------------------------------------------------------------
-- [Dashboards].[WidgetPermittedRole] -- child value collection
-- ----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'Dashboards' AND t.name = 'WidgetPermittedRole')
BEGIN
    CREATE TABLE [Dashboards].[WidgetPermittedRole]
    (
        [Id]        UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_WidgetPermittedRole_Id DEFAULT (NEWID()),
        [WidgetId]  UNIQUEIDENTIFIER NOT NULL,
        [TenantId]  UNIQUEIDENTIFIER NOT NULL,
        [RoleName]  NVARCHAR(50)     NOT NULL,

        CONSTRAINT PK_WidgetPermittedRole PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT FK_WidgetPermittedRole_Widget FOREIGN KEY ([WidgetId]) REFERENCES [Dashboards].[Widget] ([WidgetId])
    );

    CREATE NONCLUSTERED INDEX IX_WidgetPermittedRole_WidgetId ON [Dashboards].[WidgetPermittedRole] ([WidgetId]);
END
GO
