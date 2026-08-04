// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.AutonomousVehicleAsset
//  Layer:          Domain
//  File:           AutonomousVehicleAssetSnapshot.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for AutonomousVehicleAsset aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.Snapshots;

/// <summary>Immutable snapshot of an <see cref="Aggregates.AutonomousVehicleAsset"/> aggregate for fast rehydration.</summary>
public sealed record AutonomousVehicleAssetSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public AutonomousVehicleAssetId AutonomousVehicleAssetId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public AutonomousVehicleAssetStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
