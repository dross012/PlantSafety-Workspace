-- ============================================================================
--  Module:  Dashboards.Widget | Layer: Infrastructure (SQL) | Created: 2026-07-21
--  Description: Read-only stored procedures for the Widget aggregate: aggregate rehydration
--               reads and read-model DTO reads.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

-- Single aggregate row, for rehydration (no TenantId filter -- the Domain repository
-- interface intentionally carries no tenant parameter; see WidgetDapperRepository remarks).
CREATE OR ALTER PROCEDURE [Dashboards].[usp_Widget_GetById]
    @WidgetId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [WidgetId], [TenantId], [Status], [Name], [TypeCode], [DataSourceModule], [DataSourceQueryOrEndpoint],
           [RefreshIntervalSeconds], [CreatedAtUtc], [CreatedBy]
    FROM [Dashboards].[Widget]
    WHERE [WidgetId] = @WidgetId AND ISNULL([IsDeleted], 0) = 0;
END
GO

CREATE OR ALTER PROCEDURE [Dashboards].[usp_Widget_ListAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [WidgetId], [TenantId], [Status], [Name], [TypeCode], [DataSourceModule], [DataSourceQueryOrEndpoint],
           [RefreshIntervalSeconds], [CreatedAtUtc], [CreatedBy]
    FROM [Dashboards].[Widget]
    WHERE ISNULL([IsDeleted], 0) = 0;
END
GO

CREATE OR ALTER PROCEDURE [Dashboards].[usp_Widget_GetParameters]
    @WidgetId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [WidgetId], [ParamKey], [ParamValue]
    FROM [Dashboards].[WidgetParameter]
    WHERE [WidgetId] = @WidgetId;
END
GO

CREATE OR ALTER PROCEDURE [Dashboards].[usp_Widget_GetPermittedRoles]
    @WidgetId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [WidgetId], [RoleName]
    FROM [Dashboards].[WidgetPermittedRole]
    WHERE [WidgetId] = @WidgetId;
END
GO

-- Read-model DTO: single Widget by id (tenant-scoped).
CREATE OR ALTER PROCEDURE [Dashboards].[usp_Widget_GetByIdDto]
    @TenantId UNIQUEIDENTIFIER,
    @WidgetId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [WidgetId], [TenantId], [Status], [Name], [TypeCode], [DataSourceModule], [DataSourceQueryOrEndpoint],
           [RefreshIntervalSeconds]
    FROM [Dashboards].[Widget]
    WHERE [WidgetId] = @WidgetId AND [TenantId] = @TenantId AND ISNULL([IsDeleted], 0) = 0;
END
GO

-- Read-model DTO: full reusable widget library (all Active widgets) for the tenant.
CREATE OR ALTER PROCEDURE [Dashboards].[usp_Widget_GetLibrary]
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [WidgetId], [TenantId], [Status], [Name], [TypeCode], [DataSourceModule], [DataSourceQueryOrEndpoint],
           [RefreshIntervalSeconds]
    FROM [Dashboards].[Widget]
    WHERE [TenantId] = @TenantId AND [Status] = N'Active' AND ISNULL([IsDeleted], 0) = 0
    ORDER BY [Name] ASC;
END
GO

-- Existence/tenant-scope check used before resolving a Widget's normalized data contract.
CREATE OR ALTER PROCEDURE [Dashboards].[usp_Widget_Exists]
    @TenantId UNIQUEIDENTIFIER,
    @WidgetId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(*)
    FROM [Dashboards].[Widget]
    WHERE [WidgetId] = @WidgetId AND [TenantId] = @TenantId AND ISNULL([IsDeleted], 0) = 0;
END
GO
