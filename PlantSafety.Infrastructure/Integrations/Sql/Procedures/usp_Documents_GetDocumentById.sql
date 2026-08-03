-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Integrations.Documents
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Documents_GetDocumentById.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing IDocumentsRepository.GetDocumentByIdAsync.
--                  Returns zero or one row. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Integrations].[usp_Documents_GetDocumentById]
    @TenantId   UNIQUEIDENTIFIER,
    @DocumentId UNIQUEIDENTIFIER
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
          AND DocumentId = @DocumentId
          AND IsDeleted = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; DocumentId=', @DocumentId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
