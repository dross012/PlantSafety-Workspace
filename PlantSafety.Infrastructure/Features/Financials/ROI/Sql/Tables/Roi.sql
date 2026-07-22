-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.Financials.ROI
--  Layer:          Infrastructure – SQL Scripts
--  File:           Roi.sql
--  Created:        2026-07-22
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for SafetyROI calculation history, backing
--                  IRoiRepository (usp_Roi_Create/GetByFiscalYear/List).
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Financials')
BEGIN
    EXEC('CREATE SCHEMA [Financials]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Roi' AND schema_id = SCHEMA_ID('Financials'))
BEGIN
    CREATE TABLE [Financials].[Roi] (
        RoiCalculationId       UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId               UNIQUEIDENTIFIER NOT NULL,
        SiteId                 UNIQUEIDENTIFIER NULL,
        FiscalYear             INT              NOT NULL,
        TotalInvestment        DECIMAL(18,2)    NOT NULL,
        EstimatedAvoidedCosts  DECIMAL(18,2)    NOT NULL,
        NetBenefit             DECIMAL(18,2)    NOT NULL,
        RoiPercentage          DECIMAL(9,4)     NOT NULL,
        CurrencyCode           CHAR(3)          NOT NULL,
        CalculatedAtUtc        DATETIMEOFFSET(3) NOT NULL,
        IsDeleted              BIT              NOT NULL CONSTRAINT DF_Financials_Roi_IsDeleted DEFAULT (0),
        CorrelationId          UNIQUEIDENTIFIER NULL,
        CreatedAtUtc           DATETIME2(3)     NOT NULL CONSTRAINT DF_Financials_Roi_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        CreatedBy              UNIQUEIDENTIFIER NULL,
        ModifiedAtUtc          DATETIME2(3)     NULL,
        ModifiedBy             UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_Financials_Roi_TenantId_SiteId] ON [Financials].[Roi] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_Financials_Roi_FiscalYear] ON [Financials].[Roi] (TenantId, FiscalYear);
END
GO
