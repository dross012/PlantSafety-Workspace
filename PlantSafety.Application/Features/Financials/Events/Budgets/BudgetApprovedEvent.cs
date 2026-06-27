// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Integration event published when a safety budget is approved.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Events;

namespace PlantSafety.Application.Features.Financials.Events.Budgets;

/// <summary>Published via outbox when a safety budget is approved.</summary>
public sealed record BudgetApprovedEvent : IIntegrationEvent
{
    /// <inheritdoc/>
    public Guid EventId { get; init; } = Guid.CreateVersion7();

    /// <inheritdoc/>
    public string EventType => "Financials.BudgetApproved.v1";

    /// <inheritdoc/>
    public DateTimeOffset OccurredAtUtc { get; init; }

    /// <inheritdoc/>
    public Guid TenantId { get; init; }

    /// <inheritdoc/>
    public Guid CorrelationId { get; init; }

    /// <summary>Approved budget identifier.</summary>
    public required Guid BudgetId { get; init; }

    /// <summary>Site identifier.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Approved allocated amount.</summary>
    public required decimal AllocatedAmount { get; init; }

    /// <summary>ISO 4217 currency code.</summary>
    public required string CurrencyCode { get; init; }

    /// <summary>Fiscal year.</summary>
    public required int FiscalYear { get; init; }

    /// <summary>Approving actor identifier.</summary>
    public required Guid ApprovedByActorId { get; init; }
}
