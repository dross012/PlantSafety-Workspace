// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.OperatingGeofence
//  Layer:          Domain
//  File:           OperatingGeofenceFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing OperatingGeofence aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.OperatingGeofence.Factories;

/// <summary>Default factory implementation for constructing <see cref="Aggregates.OperatingGeofence"/> aggregates.</summary>
public sealed class OperatingGeofenceFactory : IOperatingGeofenceFactory
{
    /// <inheritdoc/>
    public Aggregates.OperatingGeofence Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => Aggregates.OperatingGeofence.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}
