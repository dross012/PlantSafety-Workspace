// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.AutonomousVehicleAsset
//  Layer:          Domain
//  File:           IAutonomousVehicleAssetFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing AutonomousVehicleAsset aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.Interfaces;

/// <summary>Factory interface for constructing <see cref="Aggregates.AutonomousVehicleAsset"/> aggregates.</summary>
public interface IAutonomousVehicleAssetFactory
{
    /// <summary>Creates a new AutonomousVehicleAsset aggregate with the supplied parameters.</summary>
    Aggregates.AutonomousVehicleAsset Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}
