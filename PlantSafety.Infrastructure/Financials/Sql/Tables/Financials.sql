-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Financials.Financials
--  Layer:          Infrastructure – SQL Scripts
--  File:           Financials.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Financials read-model, backing
--                  IFinancialsRepository.GetRecordsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Financials')
BEGIN
    EXEC('CREATE SCHEMA [Financials]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Financials' AND schema_id = SCHEMA_ID('Financials'))
BEGIN
    CREATE TABLE [Financials].[Financials] (
        RecordId       UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId       UNIQUEIDENTIFIER NOT NULL,
        SiteId         UNIQUEIDENTIFIER NULL,
        Category       NVARCHAR(50)     NOT NULL,
        Amount         DECIMAL(18,2)    NOT NULL,
        Status         NVARCHAR(50)     NOT NULL,
        RecordedAtUtc  DATETIME2(3)     NOT NULL,
        RecordedBy     UNIQUEIDENTIFIER NOT NULL,
        IsDeleted      BIT              NOT NULL CONSTRAINT DF_Financials_IsDeleted DEFAULT (0),
        CreatedAtUtc   DATETIME2(3)     NOT NULL CONSTRAINT DF_Financials_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        CreatedBy      UNIQUEIDENTIFIER NULL,
        ModifiedAtUtc  DATETIME2(3)     NULL,
        ModifiedBy     UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_Financials_TenantId_SiteId] ON [Financials].[Financials] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_Financials_RecordedAtUtc] ON [Financials].[Financials] (RecordedAtUtc);
END
GO
