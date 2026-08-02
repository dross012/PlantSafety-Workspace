-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.ScoreCards
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_ScoreCardsAggregate_GetById.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single ScoreCards aggregate root row by identity, tenant-scoped
--                  from day one. Backs IScoreCardsRepository.GetByIdAsync (aggregate hydration)
--                  and the Application-layer GetScoreCardsByIdQuery read path (via
--                  ScoreCardsAggregateReadDapperRepository). Canonical TRY/CATCH + ErrorDB
--                  logging (no AuditLog for reads). Projects RowVersion as a string ETag
--                  (CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1)) so callers can
--                  capture the current concurrency token for read-before-write flows. Mirrors
--                  usp_Permits_GetById.sql.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Analytics].[usp_ScoreCardsAggregate_GetById]
    @ScoreCardsId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT
            ScoreCardsId, TenantId, SiteId, Status, ScoreCardName, ReportingPeriod, KpiCategory, PerformanceIndex,
            GeneratedForSiteId, GeneratedAtUtc, PublishedByUserId, PublishedAtUtc, FinalizedByUserId, FinalizedAtUtc,
            ClosedByUserId, ClosedAtUtc, ArchivedAtUtc, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy,
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [Analytics].[ScoreCardsAggregate]
        WHERE ScoreCardsId = @ScoreCardsId AND TenantId = @TenantId AND IsDeleted = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ScoreCardsId=', @ScoreCardsId, N'; TenantId=', @TenantId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
