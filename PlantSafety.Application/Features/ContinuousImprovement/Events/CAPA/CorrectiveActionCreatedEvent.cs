// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Integration event published after a CAPA is created.
// EventType: ContinuousImprovement.CorrectiveActionCreated.v1
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.ContinuousImprovement.Events.CAPA;

/// <summary>
/// Raised after a Corrective or Preventive Action is created.
/// Consumers: Alerts (assignment notification), Analytics (open CAPA tracking).
/// </summary>
public sealed record CorrectiveActionCreatedEvent : IIntegrationEvent
{
    /// <inheritdoc/>
    public required Guid EventId { get; init; }

    /// <inheritdoc/>
    public string EventType => "ContinuousImprovement.CorrectiveActionCreated.v1";

    /// <inheritdoc/>
    public int EventVersion => 1;

    /// <inheritdoc/>
    public required Guid AggregateId { get; init; }

    /// <inheritdoc/>
    public string AggregateName => "CorrectiveAction";

    /// <inheritdoc/>
    public required Guid CorrelationId { get; init; }

    /// <inheritdoc/>
    public required Guid CausationId { get; init; }

    /// <inheritdoc/>
    public required DateTimeOffset TimestampUtc { get; init; }

    /// <inheritdoc/>
    public required Guid ActorId { get; init; }

    /// <inheritdoc/>
    public required Guid TenantId { get; init; }

    /// <summary>Site where the action is to be implemented.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Worker assigned to implement.</summary>
    public required Guid AssignedToWorkerId { get; init; }

    /// <summary>Priority of this action.</summary>
    public required string Priority { get; init; }

    /// <summary>Due date.</summary>
    public required DateTimeOffset DueAtUtc { get; init; }

    /// <summary>Source event type.</summary>
    public required string SourceEventType { get; init; }
}
