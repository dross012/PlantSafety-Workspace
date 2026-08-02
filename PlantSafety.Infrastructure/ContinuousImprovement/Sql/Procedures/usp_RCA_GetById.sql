-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ContinuousImprovement.RCA
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_RCA_GetById.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single RCA aggregate root row by identity, tenant-scoped from day
--                  one. Backs IRCARepository.GetByIdAsync (Domain write-side aggregate hydration)
--                  and the Application-layer GetRCAByIdQuery read path. Canonical TRY/CATCH +
--                  ErrorDB logging (no AuditLog for reads). Projects RowVersion as a string ETag
--                  (CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1)) so callers can
--                  capture the current concurrency token for read-before-write flows. Mirrors
--                  usp_RootCauseAnalysis_GetById.sql. Does not collide with the pre-existing
--                  usp_RCA_GetRcaRecords read-list proc (a different, coarser date-range query).
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [ContinuousImprovement].[usp_RCA_GetById]
    @RcaId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT
            RcaId, TenantId, SiteId, ProblemStatement, Methodology, Status, InitiatedAtUtc, OwnerUserId,
            StartedBy, StartedAtUtc, SubmittedBy, Findings, SubmittedAtUtc,
            ClosedBy, RootCauseSummary, ClosedAtUtc, ArchivedAtUtc,
            CreatedAtUtc, CreatedBy, ModifiedAtUtc, ModifiedBy,
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [ContinuousImprovement].[RCA]
        WHERE RcaId = @RcaId AND TenantId = @TenantId AND IsDeleted = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'RcaId=', @RcaId, N'; TenantId=', @TenantId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
