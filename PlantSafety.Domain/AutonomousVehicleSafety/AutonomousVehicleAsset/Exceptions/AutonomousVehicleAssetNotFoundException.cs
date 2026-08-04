// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.AutonomousVehicleAsset
//  Layer:          Domain
//  File:           AutonomousVehicleAssetNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when an AutonomousVehicleAsset aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.Exceptions;

/// <summary>Thrown when an <see cref="Aggregates.AutonomousVehicleAsset"/> aggregate lookup finds no match.</summary>
public sealed class AutonomousVehicleAssetNotFoundException : AutonomousVehicleAssetDomainException
{
    /// <summary>Creates a not-found exception for the given AutonomousVehicleAsset identity.</summary>
    public AutonomousVehicleAssetNotFoundException(AutonomousVehicleAssetId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"AutonomousVehicleAsset with id '{id}' was not found.",
            "AutonomousVehicleAsset.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
