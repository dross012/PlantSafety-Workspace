// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Integration event published when a safety budget is approved.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.Financials.Events.Budgets;

/// <summary>Published via outbox when a safety budget is approved.</summary>
public sealed record BudgetApprovedEvent : IIntegrationEvent
{
    /// <inheritdoc/>
    public required Guid EventId { get; init; }

    /// <inheritdoc/>
    public string EventType => "Financials.BudgetApproved.v1";

    /// <inheritdoc/>
    public int EventVersion => 1;

    /// <inheritdoc/>
    public required Guid AggregateId { get; init; }

    /// <inheritdoc/>
    public string AggregateName => "SafetyBudget";

    /// <inheritdoc/>
    public required Guid CorrelationId { get; init; }

    /// <inheritdoc/>
    public required Guid CausationId { get; init; }

    /// <inheritdoc/>
    public required DateTime TimestampUtc { get; init; }

    /// <inheritdoc/>
    public required Guid ActorId { get; init; }

    /// <inheritdoc/>
    public required Guid TenantId { get; init; }

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
