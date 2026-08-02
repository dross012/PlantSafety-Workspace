-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.ManagementOfChange
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_ManagementOfChange_GetById.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single ManagementOfChange aggregate root row by identity,
--                  tenant-scoped from day one. Backs IManagementOfChangeRepository.GetByIdAsync
--                  (aggregate hydration) and the Application-layer GetManagementOfChangeByIdQuery
--                  read path. Canonical TRY/CATCH + ErrorDB logging (no AuditLog for reads).
--                  Projects RowVersion as a string ETag
--                  (CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1)) so callers can capture
--                  the current concurrency token for read-before-write flows. Mirrors
--                  usp_Permits_GetById.sql.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_ManagementOfChange_GetById]
    @ManagementOfChangeId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT
            ManagementOfChangeId, TenantId, SiteId, Status, ChangeTitle, ChangeType, Justification, RiskLevel,
            RequestedByUserId, ApprovedByUserId, ApprovalStatus, ImplementationAtUtc, ReviewDueAtUtc,
            CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy,
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [Compliance].[ManagementOfChange]
        WHERE ManagementOfChangeId = @ManagementOfChangeId AND TenantId = @TenantId AND IsDeleted = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ManagementOfChangeId=', @ManagementOfChangeId, N'; TenantId=', @TenantId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
