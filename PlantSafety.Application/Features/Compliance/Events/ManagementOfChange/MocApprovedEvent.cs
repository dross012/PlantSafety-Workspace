// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Integration event published when a MOC request is approved.
// EventType: Compliance.MocApproved.v1
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.Compliance.Events.ManagementOfChange;

/// <summary>
/// Raised when a Management of Change request is approved for implementation.
/// Consumers: Operational (change implementation tracking), PlanningProjects (project creation trigger).
/// </summary>
public sealed record MocApprovedEvent : IIntegrationEvent
{
    /// <inheritdoc/>
    public required Guid EventId { get; init; }

    /// <inheritdoc/>
    public string EventType => "Compliance.MocApproved.v1";

    /// <inheritdoc/>
    public int EventVersion => 1;

    /// <inheritdoc/>
    public required Guid AggregateId { get; init; }

    /// <inheritdoc/>
    public string AggregateName => "ManagementOfChange";

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

    /// <summary>Site where the approved change will be implemented.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Change type.</summary>
    public required string ChangeType { get; init; }

    /// <summary>Risk level of the approved change.</summary>
    public required string RiskLevel { get; init; }
}
