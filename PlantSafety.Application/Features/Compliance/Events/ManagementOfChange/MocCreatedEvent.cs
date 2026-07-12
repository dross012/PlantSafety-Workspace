// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Integration event published when a new MOC request is created.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.Compliance.Events.ManagementOfChange;

/// <summary>Published via outbox when a new Management of Change request is created.</summary>
public sealed record MocCreatedEvent : IIntegrationEvent
{
    /// <inheritdoc/>
    public required Guid EventId { get; init; }

    /// <inheritdoc/>
    public string EventType => "Compliance.MocCreated.v1";

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
    public required DateTime TimestampUtc { get; init; }

    /// <inheritdoc/>
    public required Guid ActorId { get; init; }

    /// <inheritdoc/>
    public required Guid TenantId { get; init; }

    /// <summary>New MOC identifier.</summary>
    public required Guid MocId { get; init; }

    /// <summary>Site identifier.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Change type.</summary>
    public required string ChangeType { get; init; }

    /// <summary>Risk level.</summary>
    public required string RiskLevel { get; init; }

    /// <summary>Initiator worker identifier.</summary>
    public required Guid InitiatorId { get; init; }
}
