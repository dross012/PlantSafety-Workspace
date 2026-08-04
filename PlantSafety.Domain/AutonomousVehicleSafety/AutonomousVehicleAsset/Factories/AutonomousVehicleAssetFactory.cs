// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.AutonomousVehicleAsset
//  Layer:          Domain
//  File:           AutonomousVehicleAssetFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing AutonomousVehicleAsset aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.Factories;

/// <summary>Default factory implementation for constructing <see cref="Aggregates.AutonomousVehicleAsset"/> aggregates.</summary>
public sealed class AutonomousVehicleAssetFactory : IAutonomousVehicleAssetFactory
{
    /// <inheritdoc/>
    public Aggregates.AutonomousVehicleAsset Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => Aggregates.AutonomousVehicleAsset.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}
