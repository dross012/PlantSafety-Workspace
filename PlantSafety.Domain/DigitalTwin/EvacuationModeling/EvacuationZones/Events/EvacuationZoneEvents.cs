// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationZones
//  Layer:          Domain
//  File:           EvacuationZoneEvents.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain events raised across the real EvacuationZone lifecycle: definition,
//                  occupancy snapshot recording, threshold breach, and retirement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.Events;

/// <summary>Raised when a new evacuation zone is defined against a validated Digital Twin model version.</summary>
public sealed record EvacuationZoneDefinedEvent(
    EvacuationZoneEventId EventId,
    EvacuationZoneId EvacuationZoneId,
    string TwinModelVersion,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEvacuationZoneDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EvacuationZoneDefined";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a new occupancy reading is recorded against a zone.</summary>
public sealed record OccupancySnapshotRecordedEvent(
    EvacuationZoneEventId EventId,
    EvacuationZoneId EvacuationZoneId,
    OccupancyReadingId OccupancyReadingId,
    int HeadcountEstimate,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEvacuationZoneDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "OccupancySnapshotRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a recorded occupancy reading exceeds the zone's configured threshold. Feeds AlertRoutingService.</summary>
public sealed record OccupancyThresholdExceededEvent(
    EvacuationZoneEventId EventId,
    EvacuationZoneId EvacuationZoneId,
    OccupancyReadingId OccupancyReadingId,
    int HeadcountEstimate,
    int ThresholdHeadcount,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEvacuationZoneDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "OccupancyThresholdExceeded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when an evacuation zone is retired.</summary>
public sealed record EvacuationZoneRetiredEvent(
    EvacuationZoneEventId EventId,
    EvacuationZoneId EvacuationZoneId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEvacuationZoneDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EvacuationZoneRetired";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}
