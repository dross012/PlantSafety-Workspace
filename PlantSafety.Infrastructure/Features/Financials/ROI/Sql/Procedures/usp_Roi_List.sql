-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.Financials.ROI
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Roi_List.sql
--  Created:        2026-07-22
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only, paged stored procedure backing
--                  IRoiRepository.ListAsync. Returns TotalCount as a windowed
--                  column on every row (per RoiDapperRepository.RoiListRow
--                  shape). Additive-only evolution.
--  UPDATED:        2026-07-28 -- added ETag, projected from the RowVersion column:
--                  CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1).
-- ============================================================================

CREATE OR ALTER PROCEDURE [Financials].[usp_Roi_List]
    @TenantId      UNIQUEIDENTIFIER,
    @SiteId        UNIQUEIDENTIFIER = NULL,
    @OrderBy       NVARCHAR(100) = N'CalculatedAtUtc',
    @SortDirection NVARCHAR(4)   = N'DESC',
    @PageNumber    INT,
    @PageSize      INT
AS
SET NOCOUNT ON;
BEGIN
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    IF @Offset < 0 SET @Offset = 0;

    SELECT
        RoiCalculationId,
        TenantId,
        SiteId,
        FiscalYear,
        TotalInvestment,
        EstimatedAvoidedCosts,
        NetBenefit,
        RoiPercentage,
        CurrencyCode,
        CalculatedAtUtc,
        CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag,
        COUNT(*) OVER () AS TotalCount
    FROM [Financials].[Roi]
    WHERE TenantId = @TenantId
      AND IsDeleted = 0
      AND (@SiteId IS NULL OR SiteId = @SiteId)
    ORDER BY
        CASE WHEN @OrderBy = N'FiscalYear' AND @SortDirection = N'ASC' THEN FiscalYear END ASC,
        CASE WHEN @OrderBy = N'FiscalYear' AND @SortDirection = N'DESC' THEN FiscalYear END DESC,
        CASE WHEN @OrderBy = N'RoiPercentage' AND @SortDirection = N'ASC' THEN RoiPercentage END ASC,
        CASE WHEN @OrderBy = N'RoiPercentage' AND @SortDirection = N'DESC' THEN RoiPercentage END DESC,
        CASE WHEN (@OrderBy NOT IN (N'FiscalYear', N'RoiPercentage') OR @OrderBy IS NULL) AND @SortDirection = N'ASC' THEN CalculatedAtUtc END ASC,
        CASE WHEN (@OrderBy NOT IN (N'FiscalYear', N'RoiPercentage') OR @OrderBy IS NULL) AND @SortDirection <> N'ASC' THEN CalculatedAtUtc END DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO
