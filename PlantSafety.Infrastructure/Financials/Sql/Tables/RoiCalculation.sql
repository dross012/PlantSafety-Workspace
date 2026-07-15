-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.Financials.ROI
--  Layer:          Infrastructure - SQL Scripts
--  File:           RoiCalculation.sql
--  Created:        2026-07-15
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the SafetyROI calculation history backing IRoiRepository.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RoiCalculation' AND schema_id = SCHEMA_ID('Financials'))
BEGIN
    CREATE TABLE [Financials].[RoiCalculation] (
        [RoiCalculationId]      UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [FiscalYear]            INT                 NOT NULL,
        [TotalInvestment]       DECIMAL(18,2)       NOT NULL,
        [EstimatedAvoidedCosts] DECIMAL(18,2)       NOT NULL,
        [NetBenefit]            DECIMAL(18,2)       NOT NULL,
        [RoiPercentage]         DECIMAL(9,2)        NOT NULL,
        [CurrencyCode]          NVARCHAR(3)         NOT NULL,
        [CalculatedAtUtc]       DATETIME2(3)        NOT NULL,
        [CorrelationId]         UNIQUEIDENTIFIER    NOT NULL,
        [ActorId]               UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_RoiCalculation_TenantId_FiscalYear]
        ON [Financials].[RoiCalculation] ([TenantId], [FiscalYear] DESC)
        INCLUDE ([SiteId], [RoiPercentage], [CalculatedAtUtc]);
END
GO
