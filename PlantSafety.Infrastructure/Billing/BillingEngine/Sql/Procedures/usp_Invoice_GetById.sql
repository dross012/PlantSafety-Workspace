-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Billing.BillingEngine
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Invoice_GetById.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Phase 7 §3.3 Billing Engine)
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single Invoice by identity. Backs IInvoiceRepository.GetByIdAsync.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Billing].[usp_Invoice_GetById]
    @InvoiceId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT InvoiceId, TenantId, PeriodYear, PeriodMonth, Currency, LineItemsJson, SubtotalAmount, TotalDueAmount,
           Status, GeneratedAtUtc, IssuedAtUtc, PaidAtUtc, PaymentProcessorTransactionId
    FROM [Billing].[Invoice]
    WHERE InvoiceId = @InvoiceId;
END
GO
