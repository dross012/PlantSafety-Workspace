-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Financials
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_FinancialRecord_GetById.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single FinancialRecord aggregate root header row by identity,
--                  tenant-scoped from day one. Backs IFinancialRecordRepository.GetByIdAsync
--                  (aggregate hydration, paired with usp_FinancialRecordLineItem_GetByFinancialRecordId
--                  for the LineItems collection) and the Application-layer
--                  GetFinancialRecordByIdQuery read path. Canonical TRY/CATCH + ErrorDB logging
--                  (no AuditLog for reads). Projects RowVersion as a string ETag. Mirrors
--                  usp_AuditingAggregate_GetById.sql.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Financials].[usp_FinancialRecord_GetById]
    @FinancialRecordId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT
            FinancialRecordId, TenantId, SiteId, Status, Category, Amount,
            RecordedAtUtc, RecordedBy, UpdatedAtUtc, UpdatedBy,
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [Financials].[FinancialRecord]
        WHERE FinancialRecordId = @FinancialRecordId AND TenantId = @TenantId AND IsDeleted = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'FinancialRecordId=', @FinancialRecordId, N'; TenantId=', @TenantId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
