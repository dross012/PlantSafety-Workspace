// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.AutonomousVehicleAsset
//  Layer:          Domain
//  File:           AutonomousVehicleAssetId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the AutonomousVehicleAsset aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the AutonomousVehicleAsset aggregate root.</summary>
public readonly record struct AutonomousVehicleAssetId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a AutonomousVehicleAssetId wrapping a non-empty GUID.</summary>
    public AutonomousVehicleAssetId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AutonomousVehicleAssetId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new AutonomousVehicleAssetId using Uuid7.</summary>
    public static AutonomousVehicleAssetId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a AutonomousVehicleAssetId.</summary>
    public static AutonomousVehicleAssetId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a AutonomousVehicleAssetId.</summary>
    public static AutonomousVehicleAssetId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
