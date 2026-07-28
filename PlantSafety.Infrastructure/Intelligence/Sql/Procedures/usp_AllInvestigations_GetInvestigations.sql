-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Intelligence.AllInvestigations
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_AllInvestigations_GetInvestigations.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IAllInvestigationsRepository.GetInvestigationsAsync.
--                  Additive-only evolution.
--  UPDATED:        2026-07-28 -- wrapped in BEGIN TRY/BEGIN CATCH with a canonical ErrorLog
--                  insert (ErrorProcedure/ErrorLine/ErrorMessage/ErrorContext/... shape matching
--                  every write procedure's CATCH block, e.g. usp_Risk_Insert.sql) followed by
--                  THROW to re-raise. Deliberate, explicit exception to this codebase's normal
--                  convention (read-only Get/List procs are intentionally TRY-free elsewhere) --
--                  no AuditLog, no ROWVERSION/ETag: this module has no write path.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Intelligence].[usp_AllInvestigations_GetInvestigations]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT
            InvestigationId,
            TenantId,
            SiteId,
            IncidentReference,
            AiConfidenceScore,
            Summary,
            Status,
            OpenedAtUtc,
            AssignedTo
        FROM [Intelligence].[AllInvestigations]
        WHERE TenantId = @TenantId
          AND IsDeleted = 0
          AND (@SiteId IS NULL OR SiteId = @SiteId)
          AND OpenedAtUtc >= @FromUtc
          AND OpenedAtUtc <= @ToUtc
        ORDER BY OpenedAtUtc DESC;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; SiteId=', @SiteId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
