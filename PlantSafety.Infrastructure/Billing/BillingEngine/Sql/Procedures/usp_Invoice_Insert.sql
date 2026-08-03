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
    INSERT INTO [Billing].[Invoice]
    (InvoiceId, TenantId, PeriodYear, PeriodMonth, Currency, LineItemsJson, SubtotalAmount, TotalDueAmount,
     Status, GeneratedAtUtc, IssuedAtUtc, PaidAtUtc, PaymentProcessorTransactionId)
    VALUES
    (@InvoiceId, @TenantId, @PeriodYear, @PeriodMonth, @Currency, @LineItemsJson, @SubtotalAmount, @TotalDueAmount,
     @Status, @GeneratedAtUtc, @IssuedAtUtc, @PaidAtUtc, @PaymentProcessorTransactionId);
END
GO
