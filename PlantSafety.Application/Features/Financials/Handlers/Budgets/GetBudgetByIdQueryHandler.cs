// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetBudgetByIdQuery. Returns null when not found.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.Financials.Models.Budgets;
using PlantSafety.Application.Features.Financials.Queries.Budgets.GetBudget;

namespace PlantSafety.Application.Features.Financials.Handlers.Budgets;

/// <summary>Handles <see cref="GetBudgetByIdQuery"/>.</summary>
public sealed class GetBudgetByIdQueryHandler : IQueryHandler<GetBudgetByIdQuery, SafetyBudgetDto?>
{
    private readonly IBudgetRepository _repository;
    private readonly ILogger<GetBudgetByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetBudgetByIdQueryHandler(IBudgetRepository repository, ILogger<GetBudgetByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<SafetyBudgetDto?> Handle(GetBudgetByIdQuery request, CancellationToken cancellationToken)
    {
        var model = await _repository.GetByIdAsync(request.TenantId, request.BudgetId, cancellationToken);

        if (model is null)
        {
            _logger.LogDebug("Budget {BudgetId} not found for Tenant {TenantId}", request.BudgetId, request.TenantId);
            return null;
        }

        return new SafetyBudgetDto
        {
            BudgetId = model.BudgetId,
            TenantId = model.TenantId,
            SiteId = model.SiteId,
            Category = model.Category,
            AllocatedAmount = model.AllocatedAmount,
            ActualSpend = model.ActualSpend,
            RemainingBalance = model.RemainingBalance,
            CurrencyCode = model.CurrencyCode,
            FiscalYear = model.FiscalYear,
            Status = model.Status,
            LastModifiedAtUtc = model.LastModifiedAtUtc,
            ETag = model.ETag,
        };
    }
}
