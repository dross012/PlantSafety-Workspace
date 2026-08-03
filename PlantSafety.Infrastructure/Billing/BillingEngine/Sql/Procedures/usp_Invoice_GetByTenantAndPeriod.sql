-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Billing.BillingEngine
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Invoice_GetByTenantAndPeriod.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Phase 7 §3.3 Billing Engine)
--  SchemaVersion:  2026.1
--  Description:    Retrieves the invoice already generated for a tenant/period, if any. Backs
--                  IInvoiceRepository.GetByTenantAndPeriodAsync.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Billing].[usp_Invoice_GetByTenantAndPeriod]
    @TenantId    UNIQUEIDENTIFIER,
    @PeriodYear  INT,
    @PeriodMonth INT
AS
SET NOCOUNT ON;
BEGIN
    SELECT InvoiceId, TenantId, PeriodYear, PeriodMonth, Currency, LineItemsJson, SubtotalAmount, TotalDueAmount,
           Status, GeneratedAtUtc, IssuedAtUtc, PaidAtUtc, PaymentProcessorTransactionId
    FROM [Billing].[Invoice]
    WHERE TenantId = @TenantId AND PeriodYear = @PeriodYear AND PeriodMonth = @PeriodMonth;
END
GO
