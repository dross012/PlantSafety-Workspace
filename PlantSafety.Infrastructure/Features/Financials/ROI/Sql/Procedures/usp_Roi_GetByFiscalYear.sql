-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.Financials.ROI
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Roi_GetByFiscalYear.sql
--  Created:        2026-07-22
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IRoiRepository.GetByFiscalYearAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Financials].[usp_Roi_GetByFiscalYear]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FiscalYear INT
AS
SET NOCOUNT ON;
BEGIN
    SELECT TOP (1)
        RoiCalculationId,
        TenantId,
        SiteId,
        FiscalYear,
        TotalInvestment,
        EstimatedAvoidedCosts,
        NetBenefit,
        RoiPercentage,
        CurrencyCode,
        CalculatedAtUtc
    FROM [Financials].[Roi]
    WHERE TenantId = @TenantId
      AND IsDeleted = 0
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND FiscalYear = @FiscalYear
    ORDER BY CalculatedAtUtc DESC;
END
GO
