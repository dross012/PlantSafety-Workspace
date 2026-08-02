-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Dashboard
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Dashboard_Update.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Overwrites the full mutable state of a Dashboard aggregate root row. Backs
--                  IDashboardRepository.UpdateAsync. @TenantId is part of the WHERE clause and
--                  @ETag enforces optimistic concurrency against the native RowVersion column,
--                  mirroring usp_Permits_Update.sql. @RowsAffected is captured via @@ROWCOUNT
--                  immediately after the UPDATE (before the audit INSERT, which would otherwise
--                  reset it) and returned via SELECT -- callers MUST invoke this proc via
--                  ExecuteScalarAsync (not ExecuteNonQuery/ExecuteAsync), since SET NOCOUNT ON
--                  makes ExecuteNonQuery always return -1, which would silently disable the @ETag
--                  concurrency check.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Analytics].[usp_Dashboard_Update]
    @DashboardId            UNIQUEIDENTIFIER,
    @TenantId               UNIQUEIDENTIFIER,
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
    @UpdatedBy              UNIQUEIDENTIFIER = NULL,
    @ETag                   NVARCHAR(64) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [Analytics].[Dashboard] WHERE DashboardId = @DashboardId AND TenantId = @TenantId)
            THROW 50004, N'Dashboard not found.', 1;

        UPDATE [Analytics].[Dashboard]
        SET
            Status = @Status,
            DashboardName = @DashboardName,
            DashboardType = @DashboardType,
            OwnerId = @OwnerId,
            RefreshIntervalSeconds = @RefreshIntervalSeconds,
            WidgetCount = @WidgetCount,
            IsPublished = @IsPublished,
            LastRefreshedAtUtc = @LastRefreshedAtUtc,
            PublishedByUserId = @PublishedByUserId,
            PublishedAtUtc = @PublishedAtUtc,
            DeprecatedByUserId = @DeprecatedByUserId,
            DeprecatedAtUtc = @DeprecatedAtUtc,
            ClosedByUserId = @ClosedByUserId,
            ClosedAtUtc = @ClosedAtUtc,
            ArchivedAtUtc = @ArchivedAtUtc,
            UpdatedBy = @UpdatedBy,
            UpdatedAtUtc = SYSUTCDATETIME()
        WHERE DashboardId = @DashboardId AND TenantId = @TenantId
          AND (@ETag IS NULL OR CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag);

        DECLARE @RowsAffected INT = @@ROWCOUNT;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'Dashboard', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'DashboardId=', @DashboardId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        SELECT @RowsAffected;
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
