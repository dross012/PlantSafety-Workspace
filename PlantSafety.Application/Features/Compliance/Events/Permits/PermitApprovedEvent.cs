// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Integration event published when a Permit to Work is approved.
// EventType: Compliance.PermitApproved.v1
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.Compliance.Events.Permits;

/// <summary>
/// Raised when a Permit to Work transitions to Active status.
/// Consumers: Operational (schedule activation), Alerts (work authorisation notification).
/// </summary>
public sealed record PermitApprovedEvent : IIntegrationEvent
{
    /// <inheritdoc/>
    public required Guid EventId { get; init; }

    /// <inheritdoc/>
    public string EventType => "Compliance.PermitApproved.v1";

    /// <inheritdoc/>
    public int EventVersion => 1;

    /// <inheritdoc/>
    public required Guid AggregateId { get; init; }

    /// <inheritdoc/>
    public string AggregateName => "Permit";

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

    /// <summary>Site where the permitted work is authorised.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Permit type.</summary>
    public required string PermitType { get; init; }

    /// <summary>UTC timestamp the permit becomes valid.</summary>
    public required DateTimeOffset ValidFromUtc { get; init; }

    /// <summary>UTC timestamp the permit expires.</summary>
    public required DateTimeOffset ValidToUtc { get; init; }
}
