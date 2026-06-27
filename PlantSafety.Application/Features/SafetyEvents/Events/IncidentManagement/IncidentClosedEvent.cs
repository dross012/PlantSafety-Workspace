// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Integration event published when an Incident is closed/resolved.
//   Published via the Outbox Processor.
// EventType: SafetyEvents.IncidentClosed.v1
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.SafetyEvents.Events.IncidentManagement;

/// <summary>
/// Raised after an Incident workflow is closed (final state, no further updates).
/// Consumers: Analytics (closure rate metrics), Compliance (obligation completion check).
/// Must be idempotent on <see cref="EventId"/>.
/// </summary>
public sealed record IncidentClosedEvent : IIntegrationEvent
{
    /// <inheritdoc/>
    public required Guid EventId { get; init; }

    /// <inheritdoc/>
    public string EventType => "SafetyEvents.IncidentClosed.v1";

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

    /// <summary>UTC timestamp when the incident was formally closed.</summary>
    public required DateTimeOffset ClosedAtUtc { get; init; }

    /// <summary>Final severity classification at closure.</summary>
    public required string FinalSeverity { get; init; }

    /// <summary>Resolution notes or summary, if captured.</summary>
    public string? ResolutionNotes { get; init; }
}
