-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.Financials.Budgets
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Budget_GetById.sql
--  Created:        2026-08-06
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only single-row lookup of [Financials].[Budget] by identity. Backs
--                  IBudgetRepository.GetByIdAsync / BudgetDapperRepository.GetByIdAsync. Column
--                  list matches BudgetReadModel exactly. ETag is projected from the native
--                  ROWVERSION. TRY/CATCH + THROW only (no AuditLog/ErrorLog writes) per the
--                  established read-procedure convention. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Financials].[usp_Budget_GetById]
(
    @TenantId UNIQUEIDENTIFIER,
    @BudgetId UNIQUEIDENTIFIER
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
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [Financials].[Budget]
        WHERE BudgetId = @BudgetId
          AND TenantId = @TenantId;

        RETURN 0;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorNumber INT = ERROR_NUMBER();
        THROW;
        RETURN @ErrorNumber;
    END CATCH;
END;
GO
