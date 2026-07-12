// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Integration event published after a Safety Drill is scheduled.
// EventType: SafetyEvents.DrillScheduled.v1
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.SafetyEvents.Events.DrillsAndExercises;

/// <summary>
/// Raised after a Safety Drill is scheduled.
/// Consumers: Operational (scheduling calendar), Alerts (reminder notifications).
/// </summary>
public sealed record DrillScheduledEvent : IIntegrationEvent
{
    /// <inheritdoc/>
    public required Guid EventId { get; init; }

    /// <inheritdoc/>
    public string EventType => "SafetyEvents.DrillScheduled.v1";

    /// <inheritdoc/>
    public int EventVersion => 1;

    /// <inheritdoc/>
    public required Guid AggregateId { get; init; }

    /// <inheritdoc/>
    public string AggregateName => "Drill";

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

    /// <summary>Site where the drill is scheduled.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Drill type.</summary>
    public required string DrillType { get; init; }

    /// <summary>UTC timestamp when the drill is scheduled.</summary>
    public required DateTimeOffset ScheduledAtUtc { get; init; }
}
