-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Dashboard
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Dashboard_Insert.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Inserts a new Dashboard aggregate root row. Backs IDashboardRepository.AddAsync.
--                  TRY/CATCH with AuditDB/ErrorDB logging per the canonical write-procedure
--                  template (mirrors usp_Permits_Insert.sql). Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Analytics].[usp_Dashboard_Insert]
    @DashboardId            UNIQUEIDENTIFIER,
    @TenantId               UNIQUEIDENTIFIER,
    @SiteId                 UNIQUEIDENTIFIER = NULL,
    @Status                 NVARCHAR(30),
    @DashboardName          NVARCHAR(400),
    @DashboardType          NVARCHAR(100),
    @OwnerId                UNIQUEIDENTIFIER = NULL,
    @RefreshIntervalSeconds INT,
    @WidgetCount            INT,
    @IsPublished            BIT,
    @LastRefreshedAtUtc     DATETIME2(3) = NULL,
    @PublishedByUserId      UNIQUEIDENTIFIER = NULL,
    @PublishedAtUtc         DATETIME2(3) = NULL,
    @DeprecatedByUserId     UNIQUEIDENTIFIER = NULL,
    @DeprecatedAtUtc        DATETIME2(3) = NULL,
    @ClosedByUserId         UNIQUEIDENTIFIER = NULL,
    @ClosedAtUtc            DATETIME2(3) = NULL,
    @ArchivedAtUtc          DATETIME2(3) = NULL,
    @CreatedAtUtc           DATETIME2(3),
    @CreatedBy              UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [Analytics].[Dashboard]
        (
            DashboardId, TenantId, SiteId, Status, DashboardName, DashboardType, OwnerId,
            RefreshIntervalSeconds, WidgetCount, IsPublished, LastRefreshedAtUtc,
            PublishedByUserId, PublishedAtUtc, DeprecatedByUserId, DeprecatedAtUtc,
            ClosedByUserId, ClosedAtUtc, ArchivedAtUtc, CreatedAtUtc, CreatedBy
        )
        VALUES
        (
            @DashboardId, @TenantId, @SiteId, @Status, @DashboardName, @DashboardType, @OwnerId,
            @RefreshIntervalSeconds, @WidgetCount, @IsPublished, @LastRefreshedAtUtc,
            @PublishedByUserId, @PublishedAtUtc, @DeprecatedByUserId, @DeprecatedAtUtc,
            @ClosedByUserId, @ClosedAtUtc, @ArchivedAtUtc, @CreatedAtUtc, @CreatedBy
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'Dashboard', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'DashboardId=', @DashboardId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'DashboardId=', @DashboardId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
