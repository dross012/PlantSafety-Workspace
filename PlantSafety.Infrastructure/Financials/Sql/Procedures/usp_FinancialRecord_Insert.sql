-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Financials
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_FinancialRecord_Insert.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Inserts a new FinancialRecord aggregate root header row (Status=Draft; line
--                  items are added later via usp_FinancialRecordLineItem_Insert as they are
--                  appended to the Draft aggregate). Backs IFinancialRecordRepository.AddAsync.
--                  Distinct name/prefix from the pre-existing, unrelated
--                  [Financials].[usp_Financial_GetById] / [Financials].[usp_Financial_Insert]
--                  procs. TRY/CATCH with AuditDB/ErrorDB logging per the canonical
--                  write-procedure template (mirrors usp_AuditingAggregate_Insert.sql).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Financials].[usp_FinancialRecord_Insert]
    @FinancialRecordId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50),
    @Category NVARCHAR(200),
    @Amount DECIMAL(18,2),
    @RecordedAtUtc DATETIME2(3),
    @RecordedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [Financials].[FinancialRecord]
        (
            FinancialRecordId, TenantId, SiteId, Status, Category, Amount, RecordedAtUtc, RecordedBy, IsDeleted
        )
        VALUES
        (
            @FinancialRecordId, @TenantId, @SiteId, @Status, @Category, @Amount, @RecordedAtUtc, @RecordedBy, 0
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'FinancialRecord', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'FinancialRecordId=', @FinancialRecordId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
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
