// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.EmergencyStopDevices
//  Layer:          Domain
//  File:           EmergencyStopDevicesId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the EmergencyStopDevices aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.EmergencyStopDevices.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the EmergencyStopDevices aggregate root.</summary>
public readonly record struct EmergencyStopDevicesId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a EmergencyStopDevicesId wrapping a non-empty GUID.</summary>
    public EmergencyStopDevicesId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EmergencyStopDevicesId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new EmergencyStopDevicesId using Uuid7.</summary>
    public static EmergencyStopDevicesId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a EmergencyStopDevicesId.</summary>
    public static EmergencyStopDevicesId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a EmergencyStopDevicesId.</summary>
    public static EmergencyStopDevicesId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}