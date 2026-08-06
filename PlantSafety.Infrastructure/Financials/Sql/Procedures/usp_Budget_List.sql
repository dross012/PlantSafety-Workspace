-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.Financials.Budgets
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Budget_List.sql
--  Created:        2026-08-06
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only, paginated, filterable list of [Financials].[Budget] rows. Backs
--                  IBudgetRepository.ListAsync / BudgetDapperRepository.ListAsync. Parameter
--                  names/types mirror the StoredProcedureBuilder Add* calls in
--                  BudgetDapperRepository.ListAsync exactly. Default OrderBy = 'FiscalYear' matches
--                  ListBudgetsQuery's default. Windowed COUNT(*) OVER() AS TotalCount avoids a
--                  second round trip, matching BudgetDapperRepository.ListAsync's consumption
--                  pattern (rows[0].TotalCount). Deterministic ORDER BY + OFFSET/FETCH paging with
--                  a fixed BudgetId tiebreaker. TRY/CATCH + THROW only (no AuditLog/ErrorLog
--                  writes) per the established read-procedure convention. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Financials].[usp_Budget_List]
(
    @TenantId      UNIQUEIDENTIFIER,
    @SiteId        UNIQUEIDENTIFIER = NULL,
    @FiscalYear    INT              = NULL,
    @Status        NVARCHAR(50)     = NULL,
    @OrderBy       NVARCHAR(100)    = 'FiscalYear',
    @SortDirection NVARCHAR(4)      = 'ASC',
    @PageNumber    INT              = 1,
    @PageSize      INT              = 25
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        SELECT
            BudgetId,
            TenantId,
            SiteId,
            Category,
            AllocatedAmount,
            ActualSpend,
            RemainingBalance,
            CurrencyCode,
            FiscalYear,
            Status,
            LastModifiedAtUtc,
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag,
            COUNT(*) OVER() AS TotalCount
        FROM [Financials].[Budget]
        WHERE TenantId = @TenantId
          AND (@SiteId IS NULL OR SiteId = @SiteId)
          AND (@FiscalYear IS NULL OR FiscalYear = @FiscalYear)
          AND (@Status IS NULL OR Status = @Status)
        ORDER BY
            CASE WHEN @OrderBy = 'FiscalYear' AND @SortDirection = 'ASC'  THEN FiscalYear END ASC,
            CASE WHEN @OrderBy = 'FiscalYear' AND @SortDirection = 'DESC' THEN FiscalYear END DESC,
            CASE WHEN @OrderBy = 'Category'   AND @SortDirection = 'ASC'  THEN Category END ASC,
            CASE WHEN @OrderBy = 'Category'   AND @SortDirection = 'DESC' THEN Category END DESC,
            CASE WHEN @OrderBy = 'Status'     AND @SortDirection = 'ASC'  THEN Status END ASC,
            CASE WHEN @OrderBy = 'Status'     AND @SortDirection = 'DESC' THEN Status END DESC,
            CASE WHEN @OrderBy = 'BudgetId'   AND @SortDirection = 'DESC' THEN BudgetId END DESC,
            FiscalYear ASC, BudgetId ASC
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;

        RETURN 0;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorNumber INT = ERROR_NUMBER();
        THROW;
        RETURN @ErrorNumber;
    END CATCH;
END;
GO
