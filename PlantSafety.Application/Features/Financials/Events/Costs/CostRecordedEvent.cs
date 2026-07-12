// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Integration event published when a safety cost is recorded.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.Financials.Events.Costs;

/// <summary>Published via outbox when a safety cost entry is recorded.</summary>
public sealed record CostRecordedEvent : IIntegrationEvent
{
    /// <inheritdoc/>
    public required Guid EventId { get; init; }

    /// <inheritdoc/>
    public string EventType => "Financials.CostRecorded.v1";

    /// <inheritdoc/>
    public int EventVersion => 1;

    /// <inheritdoc/>
    public required Guid AggregateId { get; init; }

    /// <inheritdoc/>
    public string AggregateName => "SafetyCost";

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

    /// <summary>Cost entry identifier.</summary>
    public required Guid CostId { get; init; }

    /// <summary>Associated budget identifier.</summary>
    public required Guid BudgetId { get; init; }

    /// <summary>Source event identifier (e.g. incident that caused the cost).</summary>
    public required Guid? SourceEventId { get; init; }

    /// <summary>Source event type discriminator.</summary>
    public required string? SourceEventType { get; init; }

    /// <summary>Amount recorded.</summary>
    public required decimal Amount { get; init; }

    /// <summary>ISO 4217 currency code.</summary>
    public required string CurrencyCode { get; init; }

    /// <summary>Actor who recorded the cost.</summary>
    public required Guid RecordedByActorId { get; init; }
}
