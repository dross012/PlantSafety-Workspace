// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Integration event published when a Crisis Management event is declared.
// EventType: SafetyEvents.CrisisDeclared.v1
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.SafetyEvents.Events.CrisisManagement;

/// <summary>
/// Raised immediately after a crisis is declared.
/// High-priority event: Alerts module must dispatch emergency notifications within SLA.
/// Consumers: Alerts (emergency broadcast), Compliance (regulatory notification trigger).
/// </summary>
public sealed record CrisisDeclaredEvent : IIntegrationEvent
{
    /// <inheritdoc/>
    public required Guid EventId { get; init; }

    /// <inheritdoc/>
    public string EventType => "SafetyEvents.CrisisDeclared.v1";

    /// <inheritdoc/>
    public int EventVersion => 1;

    /// <inheritdoc/>
    public required Guid AggregateId { get; init; }

    /// <inheritdoc/>
    public string AggregateName => "CrisisEvent";

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

    /// <summary>Site where crisis is active.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Crisis category.</summary>
    public required string CrisisCategory { get; init; }

    /// <summary>UTC timestamp of declaration.</summary>
    public required DateTimeOffset DeclaredAtUtc { get; init; }

    /// <summary>Incident Commander.</summary>
    public required Guid IncidentCommanderId { get; init; }
}
