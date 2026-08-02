-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Culture
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Culture_GetById.sql
--  Created:        2026-07-27
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing ICultureRepository.GetByIdAsync.
--                  Additive-only evolution.
--  UPDATED:        2026-07-27 -- added ETag, projected from the new RowVersion column the same
--                  way as usp_IncidentRead_GetById.sql: CONVERT(NVARCHAR(34), CAST(RowVersion AS
--                  BINARY(8)), 1) -- the intermediate CAST is required, SQL Server rejects a
--                  direct CONVERT from rowversion/timestamp to a character type even with an
--                  explicit style code (see docs/architecture/step-b-rowversion-conversion-fix-plan.md).
--  UPDATED:        2026-08-02 -- Phase 4 full build-out: projects the new Review/Publish/Close/
--                  Archive lifecycle columns and wraps the read in the canonical TRY/CATCH +
--                  ErrorDB logging pattern, mirroring usp_Permits_GetById.sql.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Analytics].[usp_Culture_GetById]
    @TenantId  UNIQUEIDENTIFIER,
    @CultureId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT
            CultureId, TenantId, SiteId, Status, SurveyPeriod, CultureIndexScore, EngagementScore,
            ParticipationRate, TopRiskTheme, ReviewedByUserId, ReviewedAtUtc, PublishedByUserId,
            PublishedAtUtc, ClosedByUserId, ClosedAtUtc, ArchivedAtUtc, CreatedAtUtc, CreatedBy,
            UpdatedAtUtc, UpdatedBy,
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [Analytics].[Culture]
        WHERE CultureId = @CultureId AND TenantId = @TenantId;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'CultureId=', @CultureId, N'; TenantId=', @TenantId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
