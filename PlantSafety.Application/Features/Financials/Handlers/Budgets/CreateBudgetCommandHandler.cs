// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for CreateBudgetCommand.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Clock;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Utilities;
using PlantSafety.Application.Features.Financials.Commands.Budgets.CreateBudget;

namespace PlantSafety.Application.Features.Financials.Handlers.Budgets;

/// <summary>Handles <see cref="CreateBudgetCommand"/>. Returns the new BudgetId.</summary>
public sealed class CreateBudgetCommandHandler : ICommandHandler<CreateBudgetCommand, Guid>
{
    private readonly IBudgetRepository _repository;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<CreateBudgetCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateBudgetCommandHandler(
        IBudgetRepository repository,
        IDateTimeProvider clock,
        ILogger<CreateBudgetCommandHandler> logger)
    {
        _repository = repository;
        _clock = clock;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Guid> Handle(CreateBudgetCommand request, CancellationToken cancellationToken)
    {
        var budgetId = CorrelationIdGenerator.NewId(_clock.UtcNowOffset);

        await _repository.CreateAsync(
            budgetId: budgetId,
            tenantId: request.TenantId,
            siteId: request.SiteId,
            category: request.Category,
            allocatedAmount: request.AllocatedAmount,
            currencyCode: request.CurrencyCode,
            fiscalYear: request.FiscalYear,
            actorId: request.ActorId,
            correlationId: request.CorrelationId,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Budget {BudgetId} created FiscalYear {FiscalYear} for Tenant {TenantId} by Actor {ActorId}",
            budgetId, request.FiscalYear, request.TenantId, request.ActorId);

        return budgetId;
    }
}
