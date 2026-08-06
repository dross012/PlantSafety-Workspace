-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.Financials.Budgets
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Budget_Create.sql
--  Created:        2026-08-06
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Inserts a new [Financials].[Budget] row. Backs IBudgetRepository.CreateAsync /
--                  BudgetDapperRepository.CreateAsync. Parameter set matches
--                  StoredProcedureBuilder.WithCanonicalHeaders plus the explicit Add* calls in
--                  BudgetDapperRepository.CreateAsync exactly. ActualSpend starts at 0 (no write
--                  path exists for it on this feature); Status starts 'Draft'. TRY/CATCH with
--                  AuditDB/ErrorDB logging per the established pattern. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Financials].[usp_Budget_Create]
    @TenantId          UNIQUEIDENTIFIER,
    @CorrelationId     UNIQUEIDENTIFIER,
    @ActorId           UNIQUEIDENTIFIER,
    @NowUtc             DATETIME2(3),
    @BudgetId           UNIQUEIDENTIFIER,
    @SiteId             UNIQUEIDENTIFIER = NULL,
    @Category           NVARCHAR(200),
    @AllocatedAmount    DECIMAL(18,2),
    @CurrencyCode       NCHAR(3),
    @FiscalYear         INT
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [Financials].[Budget]
        (
            BudgetId, TenantId, SiteId, Category, AllocatedAmount, CurrencyCode, FiscalYear, Status,
            CreatedAtUtc, CreatedBy, LastModifiedByActorId, LastModifiedAtUtc
        )
        VALUES
        (
            @BudgetId, @TenantId, @SiteId, @Category, @AllocatedAmount, @CurrencyCode, @FiscalYear, N'Draft',
            @NowUtc, @ActorId, @ActorId, @NowUtc
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'Budget', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'BudgetId=', @BudgetId, N'; CorrelationId=', @CorrelationId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'BudgetId=', @BudgetId, N'; CorrelationId=', @CorrelationId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        THROW;
    END CATCH
END
GO
