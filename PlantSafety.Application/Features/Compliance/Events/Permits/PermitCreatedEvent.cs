// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Integration event published when a new permit-to-work is created.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Events;

namespace PlantSafety.Application.Features.Compliance.Events.Permits;

/// <summary>Published via outbox when a new permit-to-work record is created.</summary>
public sealed record PermitCreatedEvent : IIntegrationEvent
{
    /// <inheritdoc/>
    public Guid EventId { get; init; } = Guid.CreateVersion7();

    /// <inheritdoc/>
    public string EventType => "Compliance.PermitCreated.v1";

    /// <inheritdoc/>
    public DateTimeOffset OccurredAtUtc { get; init; }

    /// <inheritdoc/>
    public Guid TenantId { get; init; }

    /// <inheritdoc/>
    public Guid CorrelationId { get; init; }

    /// <summary>New permit identifier.</summary>
    public required Guid PermitId { get; init; }

    /// <summary>Site identifier.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Permit type.</summary>
    public required string PermitType { get; init; }

    /// <summary>Validity window start.</summary>
    public required DateTimeOffset ValidFromUtc { get; init; }

    /// <summary>Validity window end.</summary>
    public required DateTimeOffset ValidToUtc { get; init; }

    /// <summary>Work lead worker identifier.</summary>
    public required Guid WorkLeadId { get; init; }

    /// <summary>Actor who created the permit.</summary>
    public required Guid CreatedByActorId { get; init; }
}
