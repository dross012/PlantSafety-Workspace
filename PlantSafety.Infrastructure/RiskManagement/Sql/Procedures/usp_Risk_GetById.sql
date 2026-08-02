-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         RiskManagement
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Risk_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single Risk aggregate root row by identity. Backs
--                  IRiskRepository.GetByIdAsync. Additive-only evolution.
--  UPDATED:        2026-07-28 -- added ETag, projected from the RowVersion column:
--                  CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1).
--  UPDATED:        2026-07-29 -- added canonical TRY/CATCH + AuditDB/ErrorDB error logging (Phase
--                  4 RiskManagement query remediation; this proc now also backs the new
--                  GetRiskByIdQuery read path, not just IRiskRepository.GetByIdAsync's aggregate
--                  hydration). Read paths in this codebase don't write AuditLog entries (no
--                  mutation occurred) -- only ErrorLog on failure, matching the pattern already
--                  used by read-side procs elsewhere. Still returns exactly one row or none, with
--                  no side effects.
--  UPDATED:        2026-07-29 -- added @TenantId (Phase 4 cross-tenant access remediation): this
--                  proc previously filtered only by RiskId/IsDeleted, so any caller who knew or
--                  guessed another tenant's RiskId could read that tenant's Risk. Now requires and
--                  enforces WHERE TenantId = @TenantId; a cross-tenant lookup returns no row,
--                  which callers surface as NotFound rather than a security exception (never
--                  reveals whether the RiskId exists for a different tenant).
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [RiskMgmt].[usp_Risk_GetById]
    @RiskId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT
            RiskId, TenantId, SiteId, Status, RiskTitle, Category, Location, HazardSource, OwnerId,
            CurrentSeverity, CurrentLikelihood, Priority, DocumentedAtUtc, ResidualSeverity, ResidualLikelihood,
            LastValidatedAtUtc, ReviewCadence, LastReviewedAtUtc, NextReviewDueUtc, ReviewRequired, ReviewCount,
            RiskVersion, CreatedAtUtc, CreatedBy,
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [RiskMgmt].[Risk]
        WHERE RiskId = @RiskId AND TenantId = @TenantId AND IsDeleted = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'RiskId=', @RiskId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
