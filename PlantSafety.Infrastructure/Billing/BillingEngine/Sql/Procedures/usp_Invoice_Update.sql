-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Billing.BillingEngine
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Invoice_Update.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Phase 7 §3.3 Billing Engine)
--  SchemaVersion:  2026.1
--  Description:    Updates an existing Invoice row's lifecycle status/timestamps. Backs
--                  IInvoiceRepository.UpdateAsync.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Billing].[usp_Invoice_Update]
    @InvoiceId                     UNIQUEIDENTIFIER,
    @Status                        NVARCHAR(20),
    @IssuedAtUtc                   DATETIME2(3) = NULL,
    @PaidAtUtc                     DATETIME2(3) = NULL,
    @PaymentProcessorTransactionId NVARCHAR(200) = NULL
AS
SET NOCOUNT ON;
BEGIN
    UPDATE [Billing].[Invoice]
    SET Status = @Status,
        IssuedAtUtc = @IssuedAtUtc,
        PaidAtUtc = @PaidAtUtc,
        PaymentProcessorTransactionId = @PaymentProcessorTransactionId
    WHERE InvoiceId = @InvoiceId;
END
GO
