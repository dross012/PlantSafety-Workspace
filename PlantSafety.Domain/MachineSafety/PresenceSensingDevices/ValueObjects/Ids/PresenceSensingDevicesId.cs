// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.PresenceSensingDevices
//  Layer:          Domain
//  File:           PresenceSensingDevicesId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the PresenceSensingDevices aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.PresenceSensingDevices.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the PresenceSensingDevices aggregate root.</summary>
public readonly record struct PresenceSensingDevicesId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PresenceSensingDevicesId wrapping a non-empty GUID.</summary>
    public PresenceSensingDevicesId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PresenceSensingDevicesId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PresenceSensingDevicesId using Uuid7.</summary>
    public static PresenceSensingDevicesId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a PresenceSensingDevicesId.</summary>
    public static PresenceSensingDevicesId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a PresenceSensingDevicesId.</summary>
    public static PresenceSensingDevicesId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}