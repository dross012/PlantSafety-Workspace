// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.PresenceSensingDevices
//  Layer:          Domain
//  File:           PresenceSensingDevicesEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for PresenceSensingDevices domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.PresenceSensingDevices.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for PresenceSensingDevices domain events.</summary>
public readonly record struct PresenceSensingDevicesEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PresenceSensingDevicesEventId wrapping a non-empty GUID.</summary>
    public PresenceSensingDevicesEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PresenceSensingDevicesEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PresenceSensingDevicesEventId using Uuid7.</summary>
    public static PresenceSensingDevicesEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}