// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for ListBudgetsQuery.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Extensions;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Features.Financials.Models.Budgets;
using PlantSafety.Application.Features.Financials.Queries.Budgets.ListBudgets;

namespace PlantSafety.Application.Features.Financials.Handlers.Budgets;

/// <summary>Handles <see cref="ListBudgetsQuery"/>. Returns paginated budget records.</summary>
public sealed class ListBudgetsQueryHandler : IQueryHandler<ListBudgetsQuery, PaginatedResult<SafetyBudgetDto>>
{
    private readonly IBudgetRepository _repository;
    private readonly ILogger<ListBudgetsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListBudgetsQueryHandler(IBudgetRepository repository, ILogger<ListBudgetsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<PaginatedResult<SafetyBudgetDto>> Handle(ListBudgetsQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber.ClampPageNumber();
        var pageSize = request.PageSize.ClampPageSize();

        var (models, total) = await _repository.ListAsync(
            tenantId: request.TenantId,
            siteId: request.SiteId,
            fiscalYear: request.FiscalYear,
            status: request.Status,
            orderBy: request.OrderBy.SafeOrderBy("FiscalYear"),
            sortDirection: request.SortDirection.SafeSortDirection(),
            pageNumber: pageNumber,
            pageSize: pageSize,
            cancellationToken: cancellationToken);

        var items = models.Select(m => new SafetyBudgetDto
        {
            BudgetId = m.BudgetId,
            TenantId = m.TenantId,
            SiteId = m.SiteId,
            Title = m.Title,
            Description = m.Description,
            AllocatedAmount = m.AllocatedAmount,
            ActualSpend = m.ActualSpend,
            RemainingBalance = m.RemainingBalance,
            CurrencyCode = m.CurrencyCode,
            FiscalYear = m.FiscalYear,
            Status = m.Status,
            LastModifiedAtUtc = m.LastModifiedAtUtc,
            ETag = m.ETag,
        }).ToList();

        return new PaginatedResult<SafetyBudgetDto>(items, total, pageNumber, pageSize);
    }
}
