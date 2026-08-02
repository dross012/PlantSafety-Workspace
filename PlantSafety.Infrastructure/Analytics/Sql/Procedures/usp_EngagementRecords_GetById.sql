-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Engagement
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_EngagementRecords_GetById.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single Engagement aggregate root row by identity, tenant-scoped.
--                  Backs IEngagementRepository.GetByIdAsync (aggregate hydration) and the
--                  Application-layer GetEngagementByIdQuery read path. Canonical TRY/CATCH +
--                  ErrorDB logging (no AuditLog for reads). Projects RowVersion as a string ETag
--                  so callers can capture the current concurrency token for read-before-write
--                  flows. Mirrors usp_Permits_GetById.sql.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Analytics].[usp_EngagementRecords_GetById]
    @EngagementId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT
            EngagementId, TenantId, SiteId, Status, WorkerId, EngagementScore, SurveyPeriod, ParticipationChannel,
            ResponseCount, LastEngagedAtUtc, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy,
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [Analytics].[EngagementRecords]
        WHERE EngagementId = @EngagementId AND TenantId = @TenantId AND IsDeleted = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'EngagementId=', @EngagementId, N'; TenantId=', @TenantId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
