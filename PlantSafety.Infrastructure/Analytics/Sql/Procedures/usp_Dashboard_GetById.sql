-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Dashboard
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Dashboard_GetById.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single Dashboard aggregate root row by identity, tenant-scoped from
--                  day one. Backs IDashboardRepository.GetByIdAsync (aggregate hydration) and the
--                  Application-layer GetDashboardByIdQuery read path. Canonical TRY/CATCH +
--                  ErrorDB logging (no AuditLog for reads). Projects RowVersion as a string ETag.
--                  Mirrors usp_Permits_GetById.sql / usp_Culture_GetById.sql.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Analytics].[usp_Dashboard_GetById]
    @TenantId    UNIQUEIDENTIFIER,
    @DashboardId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT
            DashboardId, TenantId, SiteId, Status, DashboardName, DashboardType, OwnerId,
            RefreshIntervalSeconds, WidgetCount, IsPublished, LastRefreshedAtUtc,
            PublishedByUserId, PublishedAtUtc, DeprecatedByUserId, DeprecatedAtUtc,
            ClosedByUserId, ClosedAtUtc, ArchivedAtUtc, CreatedAtUtc, CreatedBy,
            UpdatedAtUtc, UpdatedBy,
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [Analytics].[Dashboard]
        WHERE DashboardId = @DashboardId AND TenantId = @TenantId;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'DashboardId=', @DashboardId, N'; TenantId=', @TenantId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
