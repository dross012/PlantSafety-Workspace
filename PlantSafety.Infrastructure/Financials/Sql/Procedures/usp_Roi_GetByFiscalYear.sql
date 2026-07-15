-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.Financials.ROI
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Roi_GetByFiscalYear.sql
--  Created:        2026-07-15
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns the most recent Safety ROI calculation for a tenant/site/fiscal year.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Financials].[usp_Roi_GetByFiscalYear]
(
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FiscalYear INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (1)
        [RoiCalculationId], [TenantId], [SiteId], [FiscalYear], [TotalInvestment],
        [EstimatedAvoidedCosts], [NetBenefit], [RoiPercentage], [CurrencyCode], [CalculatedAtUtc]
    FROM [Financials].[RoiCalculation]
    WHERE [TenantId] = @TenantId
      AND [FiscalYear] = @FiscalYear
      AND (@SiteId IS NULL OR [SiteId] = @SiteId)
    ORDER BY [CalculatedAtUtc] DESC;
END;
GO
