-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Financials
--  Layer:          Infrastructure -- SQL Scripts
--  File:           FinancialRecord.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Tables backing the new FinancialRecord aggregate root
--                  (IFinancialRecordRepository, the write-side Domain repository interface).
--                  Tenant-scoped (TenantId NOT NULL uniqueidentifier). Named "FinancialRecord"
--                  (distinct from the pre-existing, unrelated read-only [Financials].[Financials]
--                  table and the pre-existing, unrelated [Financials].[RoiCalculation] table).
--                  Includes a minimal child table (FinancialRecordLineItem) for the aggregate's
--                  LineItems collection, with a straightforward FK + delete-then-reinsert
--                  persistence strategy (no precedent for Dapper multi-row TVP/SplitOn hydration
--                  exists elsewhere in this codebase, so the simplest correct approach was
--                  chosen). Mirrors [Compliance].[AuditingAggregate]'s native ROWVERSION
--                  concurrency convention from day one.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Financials')
    EXEC('CREATE SCHEMA Financials AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FinancialRecord' AND schema_id = SCHEMA_ID('Financials'))
BEGIN
    CREATE TABLE [Financials].[FinancialRecord]
    (
        FinancialRecordId     UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_FinancialRecord PRIMARY KEY,
        TenantId               UNIQUEIDENTIFIER NOT NULL,
        SiteId                 UNIQUEIDENTIFIER NULL,
        Status                 NVARCHAR(50)     NOT NULL,
        Category               NVARCHAR(200)    NOT NULL,
        Amount                 DECIMAL(18,2)    NOT NULL,
        RecordedAtUtc          DATETIME2(3)     NOT NULL,
        RecordedBy             UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc           DATETIME2(3)     NULL,
        UpdatedBy              UNIQUEIDENTIFIER NULL,
        IsDeleted              BIT              NOT NULL CONSTRAINT DF_FinancialRecord_IsDeleted DEFAULT (0),
        RowVersion             ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_FinancialRecord_TenantId_Site ON [Financials].[FinancialRecord] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_FinancialRecord_Status ON [Financials].[FinancialRecord] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_FinancialRecord_RecordedAtUtc ON [Financials].[FinancialRecord] (RecordedAtUtc);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FinancialRecordLineItem' AND schema_id = SCHEMA_ID('Financials'))
BEGIN
    CREATE TABLE [Financials].[FinancialRecordLineItem]
    (
        LineItemId             UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_FinancialRecordLineItem PRIMARY KEY,
        FinancialRecordId     UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_FinancialRecordLineItem_FinancialRecord
            REFERENCES [Financials].[FinancialRecord] (FinancialRecordId),
        Description            NVARCHAR(400)    NOT NULL,
        Amount                 DECIMAL(18,2)    NOT NULL,
        CreatedAtUtc           DATETIME2(3)     NOT NULL CONSTRAINT DF_FinancialRecordLineItem_CreatedAtUtc DEFAULT (SYSUTCDATETIME())
    );

    CREATE NONCLUSTERED INDEX IX_FinancialRecordLineItem_FinancialRecordId ON [Financials].[FinancialRecordLineItem] (FinancialRecordId);
END
GO
