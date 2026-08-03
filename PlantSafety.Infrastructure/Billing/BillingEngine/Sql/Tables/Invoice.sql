-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Billing.BillingEngine
--  Layer:          Infrastructure -- SQL Scripts
--  File:           Invoice.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Phase 7 §3.3 Billing Engine)
--  SchemaVersion:  2026.1
--  Description:    Table backing the Invoice aggregate root. Lives in the [Billing] schema
--                  alongside [Billing].[UsageMeteringSnapshot]. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Billing')
BEGIN
    EXEC('CREATE SCHEMA [Billing]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Invoice' AND schema_id = SCHEMA_ID('Billing'))
BEGIN
    CREATE TABLE [Billing].[Invoice] (
        [InvoiceId]                     UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId]                      UNIQUEIDENTIFIER NOT NULL,
        [PeriodYear]                    INT              NOT NULL,
        [PeriodMonth]                   INT              NOT NULL,
        [Currency]                      NVARCHAR(3)      NOT NULL,
        [LineItemsJson]                 NVARCHAR(MAX)    NOT NULL,
        [SubtotalAmount]                DECIMAL(18,2)    NOT NULL,
        [TotalDueAmount]                DECIMAL(18,2)    NOT NULL,
        [Status]                        NVARCHAR(20)     NOT NULL,
        [GeneratedAtUtc]                DATETIME2(3)     NOT NULL,
        [IssuedAtUtc]                   DATETIME2(3)     NULL,
        [PaidAtUtc]                     DATETIME2(3)     NULL,
        [PaymentProcessorTransactionId] NVARCHAR(200)    NULL
    );

    CREATE UNIQUE NONCLUSTERED INDEX UX_Invoice_TenantPeriod ON [Billing].[Invoice] (TenantId, PeriodYear, PeriodMonth);
    CREATE NONCLUSTERED INDEX IX_Invoice_Tenant ON [Billing].[Invoice] (TenantId, GeneratedAtUtc DESC);
END
GO
