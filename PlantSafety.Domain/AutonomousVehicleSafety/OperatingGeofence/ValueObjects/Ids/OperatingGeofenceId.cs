// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.OperatingGeofence
//  Layer:          Domain
//  File:           OperatingGeofenceId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the OperatingGeofence aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.OperatingGeofence.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the OperatingGeofence aggregate root.</summary>
public readonly record struct OperatingGeofenceId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a OperatingGeofenceId wrapping a non-empty GUID.</summary>
    public OperatingGeofenceId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("OperatingGeofenceId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new OperatingGeofenceId using Uuid7.</summary>
    public static OperatingGeofenceId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a OperatingGeofenceId.</summary>
    public static OperatingGeofenceId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a OperatingGeofenceId.</summary>
    public static OperatingGeofenceId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
