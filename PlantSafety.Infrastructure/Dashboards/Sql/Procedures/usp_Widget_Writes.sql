-- ============================================================================
--  Module:  Dashboards.Widget | Layer: Infrastructure (SQL) | Created: 2026-07-21
--  Description: Write procedures for the Widget aggregate: Create, Activate, Configure,
--               Deprecate.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE [Dashboards].[usp_Widget_Create]
    @WidgetId                   UNIQUEIDENTIFIER,
    @TenantId                   UNIQUEIDENTIFIER,
    @Name                       NVARCHAR(200),
    @TypeCode                   NVARCHAR(30),
    @DataSourceModule           NVARCHAR(100),
    @DataSourceQueryOrEndpoint  NVARCHAR(200),
    @RefreshIntervalSeconds     INT,
    @CreatedAtUtc               DATETIME2(3),
    @CreatedBy                  UNIQUEIDENTIFIER,
    @CorrelationId              UNIQUEIDENTIFIER,
    @ActorId                    UNIQUEIDENTIFIER,
    @NowUtc                     DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        INSERT INTO [Dashboards].[Widget]
            ([WidgetId], [TenantId], [Status], [Name], [TypeCode], [DataSourceModule], [DataSourceQueryOrEndpoint],
             [RefreshIntervalSeconds], [CreatedAtUtc], [CreatedBy])
        VALUES
            (@WidgetId, @TenantId, N'Draft', @Name, @TypeCode, @DataSourceModule, @DataSourceQueryOrEndpoint,
             @RefreshIntervalSeconds, @CreatedAtUtc, @CreatedBy);
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [Dashboards].[usp_Widget_AddParameter]
    @WidgetId    UNIQUEIDENTIFIER,
    @TenantId    UNIQUEIDENTIFIER,
    @ParamKey    NVARCHAR(100),
    @ParamValue  NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        INSERT INTO [Dashboards].[WidgetParameter] ([WidgetId], [TenantId], [ParamKey], [ParamValue])
        VALUES (@WidgetId, @TenantId, @ParamKey, @ParamValue);
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [Dashboards].[usp_Widget_ClearParameters]
    @WidgetId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        DELETE FROM [Dashboards].[WidgetParameter] WHERE [WidgetId] = @WidgetId AND [TenantId] = @TenantId;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [Dashboards].[usp_Widget_AddPermittedRole]
    @WidgetId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @RoleName NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        INSERT INTO [Dashboards].[WidgetPermittedRole] ([WidgetId], [TenantId], [RoleName])
        VALUES (@WidgetId, @TenantId, @RoleName);
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [Dashboards].[usp_Widget_Activate]
    @WidgetId      UNIQUEIDENTIFIER,
    @TenantId      UNIQUEIDENTIFIER,
    @CorrelationId UNIQUEIDENTIFIER,
    @ActorId       UNIQUEIDENTIFIER,
    @NowUtc        DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        UPDATE [Dashboards].[Widget]
        SET [Status] = N'Active', [UpdatedAtUtc] = @NowUtc
        WHERE [WidgetId] = @WidgetId AND [TenantId] = @TenantId AND ISNULL([IsDeleted], 0) = 0;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [Dashboards].[usp_Widget_Configure]
    @WidgetId                  UNIQUEIDENTIFIER,
    @TenantId                  UNIQUEIDENTIFIER,
    @DataSourceModule          NVARCHAR(100) = NULL,
    @DataSourceQueryOrEndpoint NVARCHAR(200) = NULL,
    @RefreshIntervalSeconds    INT = NULL,
    @CorrelationId             UNIQUEIDENTIFIER,
    @ActorId                   UNIQUEIDENTIFIER,
    @NowUtc                    DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        UPDATE [Dashboards].[Widget]
        SET [DataSourceModule] = ISNULL(@DataSourceModule, [DataSourceModule]),
            [DataSourceQueryOrEndpoint] = ISNULL(@DataSourceQueryOrEndpoint, [DataSourceQueryOrEndpoint]),
            [RefreshIntervalSeconds] = ISNULL(@RefreshIntervalSeconds, [RefreshIntervalSeconds]),
            [UpdatedAtUtc] = @NowUtc
        WHERE [WidgetId] = @WidgetId AND [TenantId] = @TenantId AND ISNULL([IsDeleted], 0) = 0;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [Dashboards].[usp_Widget_Deprecate]
    @WidgetId           UNIQUEIDENTIFIER,
    @TenantId           UNIQUEIDENTIFIER,
    @DeprecationReason  NVARCHAR(500),
    @CorrelationId      UNIQUEIDENTIFIER,
    @ActorId            UNIQUEIDENTIFIER,
    @NowUtc             DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        UPDATE [Dashboards].[Widget]
        SET [Status] = N'Deprecated',
            [DeprecationReason] = @DeprecationReason,
            [UpdatedAtUtc] = @NowUtc
        WHERE [WidgetId] = @WidgetId AND [TenantId] = @TenantId AND ISNULL([IsDeleted], 0) = 0;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO
