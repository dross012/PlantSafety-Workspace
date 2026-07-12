// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Integration event published when a corrective or preventive action is closed.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.ContinuousImprovement.Events.CAPA;

/// <summary>Published via outbox when a CAPA record is closed.</summary>
public sealed record CorrectiveActionClosedEvent : IIntegrationEvent
{
    /// <inheritdoc/>
    public required Guid EventId { get; init; }

    /// <inheritdoc/>
    public string EventType => "ContinuousImprovement.CorrectiveActionClosed.v1";

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
    public required DateTime TimestampUtc { get; init; }

    /// <inheritdoc/>
    public required Guid ActorId { get; init; }

    /// <inheritdoc/>
    public required Guid TenantId { get; init; }

    /// <summary>CAPA identifier.</summary>
    public required Guid CaId { get; init; }

    /// <summary>Source event identifier that triggered this CAPA.</summary>
    public required Guid SourceEventId { get; init; }

    /// <summary>Source event type discriminator.</summary>
    public required string SourceEventType { get; init; }

    /// <summary>Action type: Corrective or Preventive.</summary>
    public required string ActionType { get; init; }

    /// <summary>UTC closure timestamp.</summary>
    public required DateTimeOffset ClosedAtUtc { get; init; }

    /// <summary>Actor who closed the CAPA.</summary>
    public required Guid ClosedByActorId { get; init; }

    /// <summary>Closure notes.</summary>
    public required string? ClosureNotes { get; init; }
}
