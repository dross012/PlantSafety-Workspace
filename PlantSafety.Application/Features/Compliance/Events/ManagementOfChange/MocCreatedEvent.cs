// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Integration event published when a new MOC request is created.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Events;

namespace PlantSafety.Application.Features.Compliance.Events.ManagementOfChange;

/// <summary>Published via outbox when a new Management of Change request is created.</summary>
public sealed record MocCreatedEvent : IIntegrationEvent
{
    /// <inheritdoc/>
    public Guid EventId { get; init; } = Guid.CreateVersion7();

    /// <inheritdoc/>
    public string EventType => "Compliance.MocCreated.v1";

    /// <inheritdoc/>
    public DateTimeOffset OccurredAtUtc { get; init; }

    /// <inheritdoc/>
    public Guid TenantId { get; init; }

    /// <inheritdoc/>
    public Guid CorrelationId { get; init; }

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
