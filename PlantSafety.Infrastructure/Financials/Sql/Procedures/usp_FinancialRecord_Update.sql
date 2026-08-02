-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Financials
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_FinancialRecord_Update.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Overwrites the mutable header state of a FinancialRecord aggregate root row.
--                  Backs IFinancialRecordRepository.UpdateAsync. @TenantId is part of the WHERE
--                  clause and @ETag enforces optimistic concurrency against the native RowVersion
--                  column from day one, mirroring usp_AuditingAggregate_Update.sql.
--                  @RowsAffected is captured via @@ROWCOUNT immediately after the UPDATE (before
--                  the audit INSERT) and returned via SELECT -- callers MUST invoke this proc via
--                  ExecuteScalarAsync (not ExecuteNonQuery/ExecuteAsync), since SET NOCOUNT ON
--                  makes ExecuteNonQuery always return -1, which would silently disable the
--                  @ETag concurrency check. Line-item synchronization (delete-then-reinsert) is
--                  performed by the calling repository via the separate
--                  usp_FinancialRecordLineItem_DeleteAll / usp_FinancialRecordLineItem_Insert
--                  procs within the same transaction.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Financials].[usp_FinancialRecord_Update]
    @FinancialRecordId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @Category NVARCHAR(200),
    @Amount DECIMAL(18,2),
    @UpdatedBy UNIQUEIDENTIFIER = NULL,
    @ETag NVARCHAR(64) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [Financials].[FinancialRecord] WHERE FinancialRecordId = @FinancialRecordId AND TenantId = @TenantId AND IsDeleted = 0)
            THROW 50004, N'FinancialRecord not found or has been deleted.', 1;

        UPDATE [Financials].[FinancialRecord]
        SET
            Status = @Status,
            Category = @Category,
            Amount = @Amount,
            UpdatedBy = @UpdatedBy,
            UpdatedAtUtc = SYSUTCDATETIME()
        WHERE FinancialRecordId = @FinancialRecordId AND TenantId = @TenantId AND IsDeleted = 0
          AND (@ETag IS NULL OR CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag);

        DECLARE @RowsAffected INT = @@ROWCOUNT;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'FinancialRecord', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'FinancialRecordId=', @FinancialRecordId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        SELECT @RowsAffected;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'FinancialRecordId=', @FinancialRecordId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
