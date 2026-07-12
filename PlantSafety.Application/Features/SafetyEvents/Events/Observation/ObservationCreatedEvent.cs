// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Integration event published after a Safety Observation is created.
// EventType: SafetyEvents.ObservationCreated.v1
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.SafetyEvents.Events.Observation;

/// <summary>
/// Raised after a Safety Observation is persisted.
/// Consumers: Analytics (safety culture index), Alerts (at-risk/unsafe notifications).
/// </summary>
public sealed record ObservationCreatedEvent : IIntegrationEvent
{
    /// <inheritdoc/>
    public required Guid EventId { get; init; }

    /// <inheritdoc/>
    public string EventType => "SafetyEvents.ObservationCreated.v1";

    /// <inheritdoc/>
    public int EventVersion => 1;

    /// <inheritdoc/>
    public required Guid AggregateId { get; init; }

    /// <inheritdoc/>
    public string AggregateName => "Observation";

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

    /// <summary>Site where the observation was made.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Observation type (Positive, AtRisk, Unsafe).</summary>
    public required string ObservationType { get; init; }
}
