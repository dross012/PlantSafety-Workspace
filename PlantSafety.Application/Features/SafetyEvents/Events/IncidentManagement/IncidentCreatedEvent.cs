// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Integration event published after a new Incident is committed to the database.
//   Published via the Outbox Processor (never directly from a command handler).
//   Consumers: Analytics (culture scoring), Compliance (obligation trigger), Alerts (notification dispatch).
// EventType: SafetyEvents.IncidentCreated.v1
// SchemaVersion: 1 — additive-only; never remove or rename fields.

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.SafetyEvents.Events.IncidentManagement;

/// <summary>
/// Raised after an Incident is successfully created and persisted.
/// Consumers must be idempotent on <see cref="EventId"/>.
/// </summary>
public sealed record IncidentCreatedEvent : IIntegrationEvent
{
    /// <inheritdoc/>
    public required Guid EventId { get; init; }

    /// <inheritdoc/>
    public string EventType => "SafetyEvents.IncidentCreated.v1";

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

    /// <summary>Site where the incident occurred.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Title of the incident.</summary>
    public required string Title { get; init; }

    /// <summary>Severity at time of creation.</summary>
    public required string Severity { get; init; }

    /// <summary>UTC timestamp when the incident occurred.</summary>
    public required DateTimeOffset IncidentOccurredAtUtc { get; init; }

    /// <summary>Worker who reported the incident.</summary>
    public required Guid ReportedByWorkerId { get; init; }
}
