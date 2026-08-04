// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.OperatingGeofence
//  Layer:          Domain
//  File:           OperatingGeofenceEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for OperatingGeofence domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.OperatingGeofence.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for OperatingGeofence domain events.</summary>
public readonly record struct OperatingGeofenceEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a OperatingGeofenceEventId wrapping a non-empty GUID.</summary>
    public OperatingGeofenceEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("OperatingGeofenceEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new OperatingGeofenceEventId using Uuid7.</summary>
    public static OperatingGeofenceEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
