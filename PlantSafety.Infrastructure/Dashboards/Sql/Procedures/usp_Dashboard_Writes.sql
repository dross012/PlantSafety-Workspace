-- ============================================================================
--  Module:  Dashboards.Dashboard | Layer: Infrastructure (SQL) | Created: 2026-07-21
--  Description: Write procedures for the Dashboard aggregate: Create, Publish, Archive,
--               AddWidget, RemoveWidget.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE [Dashboards].[usp_Dashboard_Create]
    @DashboardId   UNIQUEIDENTIFIER,
    @TenantId      UNIQUEIDENTIFIER,
    @SiteId        UNIQUEIDENTIFIER = NULL,
    @Name          NVARCHAR(200),
    @Description   NVARCHAR(2000),
    @CategoryCode  NVARCHAR(50),
    @Role          NVARCHAR(30),
    @CreatedAtUtc  DATETIME2(3),
    @CreatedBy     UNIQUEIDENTIFIER,
    @CorrelationId UNIQUEIDENTIFIER,
    @ActorId       UNIQUEIDENTIFIER,
    @NowUtc        DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        INSERT INTO [Dashboards].[Dashboard]
            ([DashboardId], [TenantId], [SiteId], [Status], [Name], [Description], [CategoryCode], [Role], [CreatedAtUtc], [CreatedBy])
        VALUES
            (@DashboardId, @TenantId, @SiteId, N'Draft', @Name, @Description, @CategoryCode, @Role, @CreatedAtUtc, @CreatedBy);
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [Dashboards].[usp_Dashboard_AddDefaultFilter]
    @DashboardId UNIQUEIDENTIFIER,
    @TenantId    UNIQUEIDENTIFIER,
    @FilterKey   NVARCHAR(100),
    @FilterValue NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        INSERT INTO [Dashboards].[DashboardDefaultFilter] ([DashboardId], [TenantId], [FilterKey], [FilterValue])
        VALUES (@DashboardId, @TenantId, @FilterKey, @FilterValue);
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [Dashboards].[usp_Dashboard_ClearDefaultFilters]
    @DashboardId UNIQUEIDENTIFIER,
    @TenantId    UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        DELETE FROM [Dashboards].[DashboardDefaultFilter] WHERE [DashboardId] = @DashboardId AND [TenantId] = @TenantId;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [Dashboards].[usp_Dashboard_UpdateStatus]
    @DashboardId   UNIQUEIDENTIFIER,
    @TenantId      UNIQUEIDENTIFIER,
    @Status        NVARCHAR(20),
    @CorrelationId UNIQUEIDENTIFIER,
    @ActorId       UNIQUEIDENTIFIER,
    @NowUtc        DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        UPDATE [Dashboards].[Dashboard]
        SET [Status] = @Status,
            [UpdatedAtUtc] = @NowUtc
        WHERE [DashboardId] = @DashboardId AND [TenantId] = @TenantId AND ISNULL([IsDeleted], 0) = 0;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [Dashboards].[usp_Dashboard_AddWidget]
    @PlacementId   UNIQUEIDENTIFIER,
    @DashboardId   UNIQUEIDENTIFIER,
    @TenantId      UNIQUEIDENTIFIER,
    @WidgetId      UNIQUEIDENTIFIER,
    @Row           INT,
    @Column        INT,
    @RowSpan       INT,
    @ColumnSpan    INT,
    @CorrelationId UNIQUEIDENTIFIER,
    @ActorId       UNIQUEIDENTIFIER,
    @NowUtc        DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        INSERT INTO [Dashboards].[DashboardWidgetPlacement]
            ([PlacementId], [DashboardId], [TenantId], [WidgetId], [Row], [Column], [RowSpan], [ColumnSpan], [CreatedAtUtc])
        VALUES
            (@PlacementId, @DashboardId, @TenantId, @WidgetId, @Row, @Column, @RowSpan, @ColumnSpan, @NowUtc);

        UPDATE [Dashboards].[Dashboard] SET [UpdatedAtUtc] = @NowUtc WHERE [DashboardId] = @DashboardId AND [TenantId] = @TenantId;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [Dashboards].[usp_Dashboard_AddWidgetPlacementFilterOverride]
    @PlacementId UNIQUEIDENTIFIER,
    @TenantId    UNIQUEIDENTIFIER,
    @FilterKey   NVARCHAR(100),
    @FilterValue NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        INSERT INTO [Dashboards].[DashboardWidgetPlacementFilterOverride] ([PlacementId], [TenantId], [FilterKey], [FilterValue])
        VALUES (@PlacementId, @TenantId, @FilterKey, @FilterValue);
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [Dashboards].[usp_Dashboard_RemoveWidget]
    @DashboardId   UNIQUEIDENTIFIER,
    @TenantId      UNIQUEIDENTIFIER,
    @WidgetId      UNIQUEIDENTIFIER,
    @CorrelationId UNIQUEIDENTIFIER,
    @ActorId       UNIQUEIDENTIFIER,
    @NowUtc        DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        DELETE fo
        FROM [Dashboards].[DashboardWidgetPlacementFilterOverride] fo
        JOIN [Dashboards].[DashboardWidgetPlacement] p ON p.[PlacementId] = fo.[PlacementId]
        WHERE p.[DashboardId] = @DashboardId AND p.[WidgetId] = @WidgetId AND p.[TenantId] = @TenantId;

        DELETE FROM [Dashboards].[DashboardWidgetPlacement]
        WHERE [DashboardId] = @DashboardId AND [WidgetId] = @WidgetId AND [TenantId] = @TenantId;

        UPDATE [Dashboards].[Dashboard] SET [UpdatedAtUtc] = @NowUtc WHERE [DashboardId] = @DashboardId AND [TenantId] = @TenantId;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO
