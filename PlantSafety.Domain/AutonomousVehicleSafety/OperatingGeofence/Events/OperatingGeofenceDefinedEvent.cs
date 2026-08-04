// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.OperatingGeofence
//  Layer:          Domain
//  File:           OperatingGeofenceDefinedEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain events raised across the OperatingGeofence lifecycle: definition,
//                  activation, violation detection, audit, and retirement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.OperatingGeofence.Events;

/// <summary>Raised when a new operating-domain geofence is defined.</summary>
public sealed record OperatingGeofenceDefinedEvent(
    OperatingGeofenceEventId EventId,
    OperatingGeofenceId OperatingGeofenceId,
    GeofenceZoneType ZoneType,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IOperatingGeofenceDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "OperatingGeofenceDefined";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when an autonomous asset violates an active geofence's boundary or speed limit.</summary>
public sealed record GeofenceViolationDetectedEvent(
    OperatingGeofenceEventId EventId,
    OperatingGeofenceId OperatingGeofenceId,
    Guid AutonomousVehicleAssetId,
    int ViolationCount,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IOperatingGeofenceDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "GeofenceViolationDetected";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a scheduled or ad-hoc geofence audit is recorded.</summary>
public sealed record OperatingGeofenceAuditedEvent(
    OperatingGeofenceEventId EventId,
    OperatingGeofenceId OperatingGeofenceId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IOperatingGeofenceDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "OperatingGeofenceAudited";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a geofence is retired.</summary>
public sealed record OperatingGeofenceRetiredEvent(
    OperatingGeofenceEventId EventId,
    OperatingGeofenceId OperatingGeofenceId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IOperatingGeofenceDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "OperatingGeofenceRetired";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}
