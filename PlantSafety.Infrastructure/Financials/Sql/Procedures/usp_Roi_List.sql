-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.Financials.ROI
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Roi_List.sql
--  Created:        2026-07-15
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Paginated Safety ROI calculation history for a tenant. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Financials].[usp_Roi_List]
(
    @TenantId      UNIQUEIDENTIFIER,
    @SiteId        UNIQUEIDENTIFIER = NULL,
    @OrderBy       NVARCHAR(100) = 'FiscalYear',
    @SortDirection NVARCHAR(4)   = 'DESC',
    @PageNumber    INT = 1,
    @PageSize      INT = 25
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        [RoiCalculationId], [TenantId], [SiteId], [FiscalYear], [TotalInvestment],
        [EstimatedAvoidedCosts], [NetBenefit], [RoiPercentage], [CurrencyCode], [CalculatedAtUtc],
        COUNT(*) OVER() AS [TotalCount]
    FROM [Financials].[RoiCalculation]
    WHERE [TenantId] = @TenantId
      AND (@SiteId IS NULL OR [SiteId] = @SiteId)
    ORDER BY
        CASE WHEN @OrderBy = 'FiscalYear' AND @SortDirection = 'DESC' THEN [FiscalYear] END DESC,
        CASE WHEN @OrderBy = 'FiscalYear' AND @SortDirection = 'ASC'  THEN [FiscalYear] END ASC,
        CASE WHEN @OrderBy = 'RoiPercentage' AND @SortDirection = 'DESC' THEN [RoiPercentage] END DESC,
        CASE WHEN @OrderBy = 'RoiPercentage' AND @SortDirection = 'ASC'  THEN [RoiPercentage] END ASC,
        [CalculatedAtUtc] DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
GO
