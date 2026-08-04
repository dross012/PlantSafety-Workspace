// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessPoints
//  Layer:          Domain
//  File:           AccessPointEvents.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain events raised across the AccessPoint lifecycle: registration,
//                  activation, maintenance scheduling, and decommissioning.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Events;

/// <summary>Raised when a new physical access point is registered with the system.</summary>
public sealed record AccessPointRegisteredEvent(
    AccessPointEventId EventId,
    AccessPointId AccessPointId,
    AccessPointDeviceType DeviceType,
    Guid? SafetyZoneId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAccessPointDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AccessPointRegistered";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when an access point is placed into active service.</summary>
public sealed record AccessPointActivatedEvent(
    AccessPointEventId EventId,
    AccessPointId AccessPointId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAccessPointDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AccessPointActivated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when an access point is taken offline for maintenance.</summary>
public sealed record AccessPointMaintenanceScheduledEvent(
    AccessPointEventId EventId,
    AccessPointId AccessPointId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAccessPointDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AccessPointMaintenanceScheduled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when an access point is permanently decommissioned.</summary>
public sealed record AccessPointDecommissionedEvent(
    AccessPointEventId EventId,
    AccessPointId AccessPointId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAccessPointDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AccessPointDecommissioned";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}
