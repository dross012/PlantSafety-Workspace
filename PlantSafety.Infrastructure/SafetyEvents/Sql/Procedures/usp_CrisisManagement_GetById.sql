-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.CrisisManagement
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_CrisisManagement_GetById.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single CrisisManagement aggregate root row by identity,
--                  tenant-scoped from day one. Backs ICrisisManagementRepository.GetByIdAsync
--                  (both the Domain write-side aggregate-hydration repository and the
--                  Application-layer GetCrisisManagementByIdQuery read path). Canonical
--                  TRY/CATCH + ErrorDB logging (no AuditLog for reads). Projects RowVersion as a
--                  string ETag (CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1)) so
--                  callers can capture the current concurrency token for read-before-write flows.
--                  Aliases the physical CrisisId primary-key column to CrisisManagementId so the
--                  pre-existing list proc's column name is left completely undisturbed. Mirrors
--                  usp_Permits_GetById.sql.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_CrisisManagement_GetById]
    @CrisisManagementId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT
            CrisisId AS CrisisManagementId, TenantId, SiteId, Status, Title, Severity, DeclaredAtUtc,
            RespondedBy, RespondedAtUtc, StabilizedAtUtc, ResolvedBy, ResolvedAtUtc,
            ClosedBy, ClosedAtUtc, ArchivedAtUtc, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy,
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [SafetyEvents].[CrisisManagement]
        WHERE CrisisId = @CrisisManagementId AND TenantId = @TenantId AND IsDeleted = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'CrisisManagementId=', @CrisisManagementId, N'; TenantId=', @TenantId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
