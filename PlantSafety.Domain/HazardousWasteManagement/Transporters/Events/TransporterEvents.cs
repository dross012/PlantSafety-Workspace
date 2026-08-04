// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.Transporters
//  Layer:          Domain
//  File:           TransporterEvents.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain events raised across the Transporter lifecycle: registration, pickup
//                  recording, and suspension.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.Transporters.ValueObjects.Ids;

namespace PlantSafety.Domain.HazardousWasteManagement.Transporters.Events;

/// <summary>Raised when a new transporter is registered.</summary>
public sealed record TransporterRegisteredEvent(
    TransporterEventId EventId,
    TransporterId TransporterId,
    string EpaTransporterId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ITransporterDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "TransporterRegistered";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when a transporter records a pickup against a manifest.</summary>
public sealed record TransporterPickupEvent(
    TransporterEventId EventId,
    TransporterId TransporterId,
    Guid ManifestId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ITransporterDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "TransporterPickupRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when a transporter is suspended.</summary>
public sealed record TransporterSuspendedEvent(
    TransporterEventId EventId,
    TransporterId TransporterId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ITransporterDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "TransporterSuspended";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}
