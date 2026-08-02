-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         AccidentManagement
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Accident_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single Accident aggregate root row by identity. Backs
--                  IAccidentRepository.GetByIdAsync. Additive-only evolution.
--  UPDATED:        2026-07-28 -- added ETag, projected from the new RowVersion column the same way
--                  as usp_IncidentRead_GetById.sql / usp_Culture_GetById.sql: CONVERT(NVARCHAR(34),
--                  CAST(RowVersion AS BINARY(8)), 1).
--  UPDATED:        2026-07-29 -- added canonical TRY/CATCH + ErrorDB error logging (Phase 4
--                  AccidentManagement query remediation; this proc now also backs the new
--                  GetAccidentByIdQuery read path, not just IAccidentRepository.GetByIdAsync's
--                  aggregate hydration). Matches usp_Risk_GetById's identical remediation. Still
--                  returns exactly one row or none, with no side effects.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [AccidentManagement].[usp_Accident_GetById]
    @AccidentId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT
            AccidentId, TenantId, SiteId, Status, AccidentTitle, Location, Severity, InjuredPersonCount,
            OccurredAtUtc, Category, SiteSecuredAndTriaged, InvestigatorId, InvestigatorAssignedAtUtc,
            RootCause, RootCauseNotes, RootCauseRecordedAtUtc, RegulatoryFilingRequired, RegulatoryAgency,
            RegulatoryReferenceNumber, RegulatoryFiledAtUtc, ClosedAtUtc, OwnerId, ReviewRequired,
            CreatedAtUtc, CreatedBy,
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [AccidentManagement].[Accident]
        -- App-layer tenant scoping (defense-in-depth alongside RLS, per F1 in
        -- docs\lifecycle\security\rls-audit.md): explicit WHERE clause, not reliance on RLS alone.
        WHERE AccidentId = @AccidentId AND TenantId = @TenantId AND IsDeleted = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AccidentId=', @AccidentId, N'; TenantId=', @TenantId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
