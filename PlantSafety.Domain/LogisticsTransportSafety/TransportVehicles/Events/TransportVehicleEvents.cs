// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportVehicles
//  Layer:          Domain
//  File:           TransportVehicleEvents.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain events raised across the TransportVehicle lifecycle: registration,
//                  inspection scheduling, inspection outcome, and retirement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.Events;

/// <summary>Raised when a new transport vehicle is registered.</summary>
public sealed record TransportVehicleRegisteredEvent(
    TransportVehicleEventId EventId,
    TransportVehicleId TransportVehicleId,
    TransportVehicleType VehicleType,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ITransportVehicleDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "TransportVehicleRegistered";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when the next safety inspection is scheduled.</summary>
public sealed record TransportVehicleInspectionScheduledEvent(
    TransportVehicleEventId EventId,
    TransportVehicleId TransportVehicleId,
    UtcTimestamp DueAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ITransportVehicleDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "TransportVehicleInspectionScheduled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => DueAtUtc;
}

/// <summary>Raised when an inspection result is recorded.</summary>
public sealed record TransportVehicleInspectionRecordedEvent(
    TransportVehicleEventId EventId,
    TransportVehicleId TransportVehicleId,
    bool Passed,
    decimal InspectionScore,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ITransportVehicleDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "TransportVehicleInspectionRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when the vehicle is retired.</summary>
public sealed record TransportVehicleRetiredEvent(
    TransportVehicleEventId EventId,
    TransportVehicleId TransportVehicleId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ITransportVehicleDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "TransportVehicleRetired";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}
