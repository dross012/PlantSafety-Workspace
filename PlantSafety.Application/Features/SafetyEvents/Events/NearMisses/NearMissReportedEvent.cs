// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Integration event published after a Near Miss is created.
// EventType: SafetyEvents.NearMissReported.v1
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.SafetyEvents.Events.NearMisses;

/// <summary>
/// Raised after a Near Miss is reported and persisted.
/// Consumers: Analytics (leading indicator metrics), Alerts (hazard notification).
/// </summary>
public sealed record NearMissReportedEvent : IIntegrationEvent
{
    /// <inheritdoc/>
    public required Guid EventId { get; init; }

    /// <inheritdoc/>
    public string EventType => "SafetyEvents.NearMissReported.v1";

    /// <inheritdoc/>
    public int EventVersion => 1;

    /// <inheritdoc/>
    public required Guid AggregateId { get; init; }

    /// <inheritdoc/>
    public string AggregateName => "NearMiss";

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

    /// <summary>Site where the near miss occurred.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Potential severity classification.</summary>
    public required string PotentialSeverity { get; init; }

    /// <summary>Hazard category.</summary>
    public required string HazardCategory { get; init; }
}
