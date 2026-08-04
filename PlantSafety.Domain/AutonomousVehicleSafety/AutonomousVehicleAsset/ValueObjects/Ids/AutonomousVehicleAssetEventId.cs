// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.AutonomousVehicleAsset
//  Layer:          Domain
//  File:           AutonomousVehicleAssetEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for AutonomousVehicleAsset domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for AutonomousVehicleAsset domain events.</summary>
public readonly record struct AutonomousVehicleAssetEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a AutonomousVehicleAssetEventId wrapping a non-empty GUID.</summary>
    public AutonomousVehicleAssetEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AutonomousVehicleAssetEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new AutonomousVehicleAssetEventId using Uuid7.</summary>
    public static AutonomousVehicleAssetEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
