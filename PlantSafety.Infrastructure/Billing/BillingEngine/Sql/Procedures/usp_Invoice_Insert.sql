-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Billing.BillingEngine
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Invoice_Insert.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Phase 7 §3.3 Billing Engine)
--  SchemaVersion:  2026.1
--  Description:    Inserts a newly generated Invoice row. Backs IInvoiceRepository.AddAsync.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Billing].[usp_Invoice_Insert]
    @InvoiceId                     UNIQUEIDENTIFIER,
    @TenantId                      UNIQUEIDENTIFIER,
    @PeriodYear                    INT,
    @PeriodMonth                   INT,
    @Currency                      NVARCHAR(3),
    @LineItemsJson                 NVARCHAR(MAX),
    @SubtotalAmount                DECIMAL(18,2),
    @TotalDueAmount                DECIMAL(18,2),
    @Status                        NVARCHAR(20),
    @GeneratedAtUtc                DATETIME2(3),
    @IssuedAtUtc                   DATETIME2(3) = NULL,
    @PaidAtUtc                     DATETIME2(3) = NULL,
    @PaymentProcessorTransactionId NVARCHAR(200) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
    INSERT INTO [Billing].[Invoice]
    (InvoiceId, TenantId, PeriodYear, PeriodMonth, Currency, LineItemsJson, SubtotalAmount, TotalDueAmount,
     Status, GeneratedAtUtc, IssuedAtUtc, PaidAtUtc, PaymentProcessorTransactionId)
    VALUES
    (@InvoiceId, @TenantId, @PeriodYear, @PeriodMonth, @Currency, @LineItemsJson, @SubtotalAmount, @TotalDueAmount,
     @Status, @GeneratedAtUtc, @IssuedAtUtc, @PaidAtUtc, @PaymentProcessorTransactionId);

        -- Canonical audit trail (Handbook section 2.11): recorded after the write, in its own
        -- TRY/CATCH so an audit failure can never fail the operation it describes.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'Invoice', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'InvoiceId=', @InvoiceId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'InvoiceId=', @InvoiceId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
            -- Error logging must never mask the original error
        END CATCH;

        THROW;
    END CATCH
END
GO
