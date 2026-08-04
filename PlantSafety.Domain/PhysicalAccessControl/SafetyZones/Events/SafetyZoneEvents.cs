// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.SafetyZones
//  Layer:          Domain
//  File:           SafetyZoneEvents.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain events raised across the SafetyZone lifecycle: registration,
//                  restriction, deactivation, and occupancy updates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Events;

/// <summary>Raised when a new safety zone is registered for occupancy correlation.</summary>
public sealed record SafetyZoneRegisteredEvent(
    SafetyZoneEventId EventId,
    SafetyZoneId SafetyZoneId,
    SafetyZoneHazardLevel HazardLevel,
    int MaxOccupancy,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ISafetyZoneDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "SafetyZoneRegistered";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a safety zone is placed under a restricted-access policy.</summary>
public sealed record SafetyZoneRestrictedEvent(
    SafetyZoneEventId EventId,
    SafetyZoneId SafetyZoneId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ISafetyZoneDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "SafetyZoneRestricted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a safety zone is deactivated (no longer occupancy-tracked).</summary>
public sealed record SafetyZoneDeactivatedEvent(
    SafetyZoneEventId EventId,
    SafetyZoneId SafetyZoneId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ISafetyZoneDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "SafetyZoneDeactivated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when current occupancy is recalculated from ingested AccessEvents.</summary>
public sealed record SafetyZoneOccupancyUpdatedEvent(
    SafetyZoneEventId EventId,
    SafetyZoneId SafetyZoneId,
    int CurrentOccupancy,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ISafetyZoneDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "SafetyZoneOccupancyUpdated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}
