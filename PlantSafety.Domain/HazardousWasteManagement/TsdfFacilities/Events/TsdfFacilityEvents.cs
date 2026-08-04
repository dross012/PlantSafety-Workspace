// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.TsdfFacilities
//  Layer:          Domain
//  File:           TsdfFacilityEvents.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain events raised across the TsdfFacility lifecycle: registration, receipt
//                  recording, and suspension.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.TsdfFacilities.ValueObjects.Ids;

namespace PlantSafety.Domain.HazardousWasteManagement.TsdfFacilities.Events;

/// <summary>Raised when a new TSDF facility is registered.</summary>
public sealed record TsdfFacilityRegisteredEvent(
    TsdfFacilityEventId EventId,
    TsdfFacilityId TsdfFacilityId,
    string EpaFacilityId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ITsdfFacilityDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "TsdfFacilityRegistered";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when a TSDF facility records receipt of a manifested shipment.</summary>
public sealed record TsdfReceiptEvent(
    TsdfFacilityEventId EventId,
    TsdfFacilityId TsdfFacilityId,
    Guid ManifestId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ITsdfFacilityDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "TsdfReceiptRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when a TSDF facility is suspended.</summary>
public sealed record TsdfFacilitySuspendedEvent(
    TsdfFacilityEventId EventId,
    TsdfFacilityId TsdfFacilityId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ITsdfFacilityDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "TsdfFacilitySuspended";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}
