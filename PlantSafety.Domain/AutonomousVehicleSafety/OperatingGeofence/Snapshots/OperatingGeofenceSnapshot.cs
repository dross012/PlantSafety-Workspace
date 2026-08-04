// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.OperatingGeofence
//  Layer:          Domain
//  File:           OperatingGeofenceSnapshot.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for OperatingGeofence aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.OperatingGeofence.Snapshots;

/// <summary>Immutable snapshot of an <see cref="Aggregates.OperatingGeofence"/> aggregate for fast rehydration.</summary>
public sealed record OperatingGeofenceSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public OperatingGeofenceId OperatingGeofenceId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public OperatingGeofenceStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
