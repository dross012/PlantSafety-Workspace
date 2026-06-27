// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Integration event published when a safety cost is recorded.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Events;

namespace PlantSafety.Application.Features.Financials.Events.Costs;

/// <summary>Published via outbox when a safety cost entry is recorded.</summary>
public sealed record CostRecordedEvent : IIntegrationEvent
{
    /// <inheritdoc/>
    public Guid EventId { get; init; } = Guid.CreateVersion7();

    /// <inheritdoc/>
    public string EventType => "Financials.CostRecorded.v1";

    /// <inheritdoc/>
    public DateTimeOffset OccurredAtUtc { get; init; }

    /// <inheritdoc/>
    public Guid TenantId { get; init; }

    /// <inheritdoc/>
    public Guid CorrelationId { get; init; }

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
