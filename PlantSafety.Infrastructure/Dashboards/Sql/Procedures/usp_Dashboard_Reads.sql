-- ============================================================================
--  Module:  Dashboards.Dashboard | Layer: Infrastructure (SQL) | Created: 2026-07-21
--  Description: Read-only stored procedures for the Dashboard aggregate: aggregate rehydration
--               reads and read-model DTO reads.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

-- Single aggregate row, for rehydration (no TenantId filter -- the Domain repository
-- interface intentionally carries no tenant parameter; see DashboardDapperRepository remarks).
CREATE OR ALTER PROCEDURE [Dashboards].[usp_Dashboard_GetById]
    @DashboardId UNIQUEIDENTIFIER,
    @TenantId    UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [DashboardId], [TenantId], [SiteId], [Status], [Name], [Description], [CategoryCode], [Role],
           [CreatedAtUtc], [CreatedBy]
    FROM [Dashboards].[Dashboard]
    WHERE [DashboardId] = @DashboardId AND ISNULL([IsDeleted], 0) = 0;
END
GO

CREATE OR ALTER PROCEDURE [Dashboards].[usp_Dashboard_ListAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [DashboardId], [TenantId], [SiteId], [Status], [Name], [Description], [CategoryCode], [Role],
           [CreatedAtUtc], [CreatedBy]
    FROM [Dashboards].[Dashboard]
    WHERE ISNULL([IsDeleted], 0) = 0;
END
GO

CREATE OR ALTER PROCEDURE [Dashboards].[usp_Dashboard_GetDefaultFilters]
    @DashboardId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [DashboardId], [FilterKey], [FilterValue]
    FROM [Dashboards].[DashboardDefaultFilter]
    WHERE [DashboardId] = @DashboardId;
END
GO

CREATE OR ALTER PROCEDURE [Dashboards].[usp_Dashboard_GetWidgetLayout]
    @DashboardId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [PlacementId], [DashboardId], [WidgetId], [Row], [Column], [RowSpan], [ColumnSpan]
    FROM [Dashboards].[DashboardWidgetPlacement]
    WHERE [DashboardId] = @DashboardId AND ISNULL([IsDeleted], 0) = 0;
END
GO

CREATE OR ALTER PROCEDURE [Dashboards].[usp_Dashboard_GetPlacementFilterOverrides]
    @DashboardId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT fo.[PlacementId], fo.[FilterKey], fo.[FilterValue]
    FROM [Dashboards].[DashboardWidgetPlacementFilterOverride] fo
    JOIN [Dashboards].[DashboardWidgetPlacement] p ON p.[PlacementId] = fo.[PlacementId]
    WHERE p.[DashboardId] = @DashboardId AND ISNULL(p.[IsDeleted], 0) = 0;
END
GO

-- Read-model DTO: single Dashboard by id (tenant-scoped).
CREATE OR ALTER PROCEDURE [Dashboards].[usp_Dashboard_GetByIdDto]
    @TenantId    UNIQUEIDENTIFIER,
    @DashboardId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [DashboardId], [TenantId], [SiteId], [Status], [Name], [Description], [CategoryCode], [Role]
    FROM [Dashboards].[Dashboard]
    WHERE [DashboardId] = @DashboardId AND [TenantId] = @TenantId AND ISNULL([IsDeleted], 0) = 0;
END
GO

-- Read-model DTO: all published Dashboards visible to the given viewer role.
CREATE OR ALTER PROCEDURE [Dashboards].[usp_Dashboard_GetForRole]
    @TenantId UNIQUEIDENTIFIER,
    @Role     NVARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [DashboardId], [TenantId], [SiteId], [Status], [Name], [Description], [CategoryCode], [Role]
    FROM [Dashboards].[Dashboard]
    WHERE [TenantId] = @TenantId
      AND [Role] = @Role
      AND [Status] = N'Published'
      AND ISNULL([IsDeleted], 0) = 0
    ORDER BY [Name] ASC;
END
GO
