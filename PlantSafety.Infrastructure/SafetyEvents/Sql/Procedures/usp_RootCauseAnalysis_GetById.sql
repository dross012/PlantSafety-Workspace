-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.RootCauseAnalysis
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_RootCauseAnalysis_GetById.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single RootCauseAnalysis aggregate root row by identity,
--                  tenant-scoped from day one. Backs IRootCauseAnalysisRepository.GetByIdAsync
--                  (Domain write-side aggregate hydration) and the Application-layer
--                  GetRootCauseAnalysisByIdQuery read path. Canonical TRY/CATCH + ErrorDB logging
--                  (no AuditLog for reads). Projects RowVersion as a string ETag (CONVERT(NVARCHAR(34),
--                  CAST(RowVersion AS BINARY(8)), 1)) so callers can capture the current
--                  concurrency token for read-before-write flows. Mirrors usp_Permits_GetById.sql /
--                  usp_PredictiveMaintenanceAlert_GetById.sql. AnalysisId is aliased to
--                  RootCauseAnalysisId in the projection to match the aggregate's own property name.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_RootCauseAnalysis_GetById]
    @RootCauseAnalysisId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT
            AnalysisId AS RootCauseAnalysisId, TenantId, SiteId, Status, RelatedEventId, Methodology, LeadAnalyst,
            SubmittedBy, SubmittedAtUtc, ApprovedBy, CompletedAtUtc,
            ClosedBy, ClosedAtUtc, ArchivedAtUtc, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy,
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [SafetyEvents].[RootCauseAnalysis]
        WHERE AnalysisId = @RootCauseAnalysisId AND TenantId = @TenantId AND IsDeleted = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'RootCauseAnalysisId=', @RootCauseAnalysisId, N'; TenantId=', @TenantId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
