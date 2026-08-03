-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Integrations.Documents
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Documents_ListDocumentsPage.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure returning one page of rows for
--                  IDocumentsRepository.ListDocumentsAsync, mirroring the Alerts.Alert
--                  usp_Alert_GetHistoryCount / usp_Alert_GetHistoryPage two-procedure pagination
--                  pattern. Deterministic ORDER BY per §1.5 (Deterministic Pagination Rule).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Integrations].[usp_Documents_ListDocumentsPage]
    @TenantId     UNIQUEIDENTIFIER,
    @SiteId       UNIQUEIDENTIFIER = NULL,
    @DocumentType NVARCHAR(100)    = NULL,
    @Search       NVARCHAR(200)    = NULL,
    @PageNumber   INT              = 1,
    @PageSize     INT              = 25
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT
            DocumentId,
            TenantId,
            SiteId,
            SourceSystem,
            FileName,
            DocumentType,
            Status,
            SyncedAtUtc,
            SyncedBy
        FROM [Integrations].[DocumentsItems]
        WHERE TenantId = @TenantId
          AND (@SiteId IS NULL OR SiteId = @SiteId)
          AND IsDeleted = 0
          AND (@DocumentType IS NULL OR DocumentType = @DocumentType)
          AND (@Search IS NULL OR FileName LIKE N'%' + @Search + N'%')
        ORDER BY SyncedAtUtc DESC, DocumentId ASC
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;
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
