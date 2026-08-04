// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.AutonomousVehicleAsset
//  Layer:          Domain
//  File:           AutonomousVehicleAssetInvalidStatusTransitionException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when an AutonomousVehicleAsset lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.Exceptions;

/// <summary>Thrown when an <see cref="Aggregates.AutonomousVehicleAsset"/> lifecycle transition is not permitted.</summary>
public sealed class AutonomousVehicleAssetInvalidStatusTransitionException : AutonomousVehicleAssetDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public AutonomousVehicleAssetInvalidStatusTransitionException(
        AutonomousVehicleAssetId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'AutonomousVehicleAsset' {id} from '{from}' to '{to}'.",
            "AutonomousVehicleAsset.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
