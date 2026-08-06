-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.Financials.Budgets
--  Layer:          Infrastructure – SQL Scripts
--  File:           Budget.sql
--  Created:        2026-08-06
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the [Financials].[Budget] (SafetyBudget) aggregate, backing
--                  IBudgetRepository / BudgetDapperRepository (PlantSafety.Infrastructure.Features.
--                  Financials.Budgets.Repositories). Confirmed real, reachable via
--                  BudgetsV1Controller; no usp_Budget_* existed anywhere in source. Column list
--                  mirrors BudgetReadModel and the StoredProcedureBuilder Add* calls in
--                  BudgetDapperRepository exactly. IBudgetRepository has no Update method, so
--                  ActualSpend has no write path from this feature -- starts at 0.
--                  RemainingBalance is a persisted computed column (AllocatedAmount - ActualSpend)
--                  rather than a plain column, so it never drifts out of sync with the two inputs
--                  it derives from. Status is documented on BudgetReadModel as
--                  Draft/Active/Approved/Closed; CreateAsync never sets it, so new rows start at
--                  'Draft' (first in that sequence). Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Financials')
BEGIN
    EXEC('CREATE SCHEMA [Financials]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Budget' AND schema_id = SCHEMA_ID('Financials'))
BEGIN
    CREATE TABLE [Financials].[Budget] (
        BudgetId              UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Budget PRIMARY KEY,
        TenantId               UNIQUEIDENTIFIER NOT NULL,
        SiteId                 UNIQUEIDENTIFIER NULL,
        Category               NVARCHAR(200)    NOT NULL,
        AllocatedAmount        DECIMAL(18,2)    NOT NULL,
        ActualSpend            DECIMAL(18,2)    NOT NULL CONSTRAINT DF_Budget_ActualSpend DEFAULT (0),
        RemainingBalance       AS (AllocatedAmount - ActualSpend) PERSISTED,
        CurrencyCode           NCHAR(3)         NOT NULL,
        FiscalYear             INT              NOT NULL,
        Status                 NVARCHAR(50)     NOT NULL CONSTRAINT DF_Budget_Status DEFAULT (N'Draft'),
        CreatedAtUtc           DATETIME2(3)     NOT NULL,
        CreatedBy              UNIQUEIDENTIFIER NOT NULL,
        LastModifiedByActorId  UNIQUEIDENTIFIER NULL,
        LastModifiedAtUtc      DATETIME2(3)     NOT NULL,
        RowVersion             ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_Budget_TenantId_FiscalYear ON [Financials].[Budget] (TenantId, FiscalYear);
    CREATE NONCLUSTERED INDEX IX_Budget_TenantId_SiteId ON [Financials].[Budget] (TenantId, SiteId);
END
GO
