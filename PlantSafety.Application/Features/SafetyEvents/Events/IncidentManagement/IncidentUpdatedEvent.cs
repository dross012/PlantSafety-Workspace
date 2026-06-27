// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Integration event published after an Incident record is updated.
//   Published via the Outbox Processor.
// EventType: SafetyEvents.IncidentUpdated.v1
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.SafetyEvents.Events.IncidentManagement;

/// <summary>
/// Raised after an Incident is successfully updated.
/// Consumers must be idempotent on <see cref="EventId"/>.
/// </summary>
public sealed record IncidentUpdatedEvent : IIntegrationEvent
{
    /// <inheritdoc/>
    public required Guid EventId { get; init; }

    /// <inheritdoc/>
    public string EventType => "SafetyEvents.IncidentUpdated.v1";

    /// <inheritdoc/>
    public int EventVersion => 1;

    /// <inheritdoc/>
    public required Guid AggregateId { get; init; }

    /// <inheritdoc/>
    public string AggregateName => "Incident";

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

    // --- Domain payload ---

    /// <summary>Updated title, if changed.</summary>
    public string? Title { get; init; }

    /// <summary>Updated severity, if changed.</summary>
    public string? Severity { get; init; }

    /// <summary>Updated status, if changed.</summary>
    public string? Status { get; init; }

    /// <summary>Updated assigned investigator, if changed.</summary>
    public Guid? AssignedInvestigatorId { get; init; }
}
