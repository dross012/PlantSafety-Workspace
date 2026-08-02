-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Financials
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_FinancialRecordLineItem_GetByFinancialRecordId.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Retrieves every FinancialRecordLineItem row for the given FinancialRecordId,
--                  ordered deterministically by CreatedAtUtc. Backs
--                  IFinancialRecordRepository.GetByIdAsync's LineItems hydration. Canonical
--                  TRY/CATCH + ErrorDB logging (no AuditLog for reads).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Financials].[usp_FinancialRecordLineItem_GetByFinancialRecordId]
    @FinancialRecordId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT LineItemId, FinancialRecordId, Description, Amount, CreatedAtUtc
        FROM [Financials].[FinancialRecordLineItem]
        WHERE FinancialRecordId = @FinancialRecordId
        ORDER BY CreatedAtUtc ASC, LineItemId ASC;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'FinancialRecordId=', @FinancialRecordId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
