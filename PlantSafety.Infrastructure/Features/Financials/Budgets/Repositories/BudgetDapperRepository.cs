// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Features.Financials.Budgets
//  Layer:          Infrastructure
//  File:           BudgetDapperRepository.cs
//  Created:        2026-07-12
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the SafetyBudget aggregate. All DB access via stored
//                  procedures in the [Financials] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Features.Financials.Handlers.Budgets;

namespace PlantSafety.Infrastructure.Features.Financials.Budgets.Repositories;

/// <summary>
/// Dapper-backed repository for the <c>SafetyBudget</c> aggregate.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class BudgetDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    DapperQueryExecutor queryExecutor,
    ILogger<BudgetDapperRepository> logger)
    : IBudgetRepository
{
    /// <inheritdoc/>
    public async Task CreateAsync(
        Guid budgetId,
        Guid tenantId,
        Guid? siteId,
        string category,
        decimal allocatedAmount,
        string currencyCode,
        int fiscalYear,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Create Budget {BudgetId} for tenant {TenantId}", budgetId, tenantId);
        using var conn = connectionFactory.CreateConnection();
        using var tx   = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .WithCanonicalHeaders(tenantId, correlationId, actorId, DateTime.UtcNow)
            .AddGuid("BudgetId", budgetId)
            .AddGuid("SiteId", siteId)
            .AddString("Category", category, 200)
            .AddDecimal("AllocatedAmount", allocatedAmount)
            .AddString("CurrencyCode", currencyCode, 3)
            .AddInt("FiscalYear", fiscalYear)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Financials].[usp_Budget_Create]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();
    }

    /// <inheritdoc/>
    public async Task<BudgetReadModel?> GetByIdAsync(Guid tenantId, Guid budgetId, CancellationToken cancellationToken)
    {
        logger.LogDebug("Get Budget {BudgetId} for tenant {TenantId}", budgetId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("BudgetId", budgetId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<BudgetReadModel>(
            "[Financials].[usp_Budget_GetById]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<BudgetReadModel> Items, int TotalCount)> ListAsync(
        Guid tenantId,
        Guid? siteId,
        int? fiscalYear,
        string? status,
        string orderBy,
        string sortDirection,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("List Budgets for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddInt("FiscalYear", fiscalYear)
            .AddString("Status", status, 50)
            .AddString("OrderBy", orderBy, 100)
            .AddString("SortDirection", sortDirection, 4)
            .AddInt("PageNumber", pageNumber)
            .AddInt("PageSize", pageSize)
            .Build();

        var rows = await queryExecutor.QueryAsync<BudgetListRow>(
            "[Financials].[usp_Budget_List]", parameters, cancellationToken);

        var items = rows.Select(r => new BudgetReadModel
        {
            BudgetId = r.BudgetId,
            TenantId = r.TenantId,
            SiteId = r.SiteId,
            Category = r.Category,
            AllocatedAmount = r.AllocatedAmount,
            ActualSpend = r.ActualSpend,
            RemainingBalance = r.RemainingBalance,
            CurrencyCode = r.CurrencyCode,
            FiscalYear = r.FiscalYear,
            Status = r.Status,
            LastModifiedAtUtc = r.LastModifiedAtUtc,
            ETag = r.ETag,
        }).ToList();

        var totalCount = rows.Count > 0 ? rows[0].TotalCount : 0;
        return (items, totalCount);
    }

    /// <summary>Raw row shape returned by <c>[Financials].[usp_Budget_List]</c>, including the windowed total row count.</summary>
    private sealed record BudgetListRow
    {
        public required Guid BudgetId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Category { get; init; }
        public required decimal AllocatedAmount { get; init; }
        public required decimal ActualSpend { get; init; }
        public required decimal RemainingBalance { get; init; }
        public required string CurrencyCode { get; init; }
        public required int FiscalYear { get; init; }
        public required string Status { get; init; }
        public required DateTimeOffset LastModifiedAtUtc { get; init; }
        public required string ETag { get; init; }
        public required int TotalCount { get; init; }
    }
}
