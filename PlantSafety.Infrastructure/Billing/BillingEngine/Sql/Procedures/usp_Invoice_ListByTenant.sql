-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Billing.BillingEngine
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Invoice_ListByTenant.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Phase 7 §3.3 Billing Engine)
--  SchemaVersion:  2026.1
--  Description:    Lists every invoice generated for a tenant, most recent period first. Backs
--                  IInvoiceRepository.ListByTenantAsync.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Billing].[usp_Invoice_ListByTenant]
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT InvoiceId, TenantId, PeriodYear, PeriodMonth, Currency, LineItemsJson, SubtotalAmount, TotalDueAmount,
           Status, GeneratedAtUtc, IssuedAtUtc, PaidAtUtc, PaymentProcessorTransactionId
    FROM [Billing].[Invoice]
    WHERE TenantId = @TenantId
    ORDER BY PeriodYear DESC, PeriodMonth DESC;
END
GO
