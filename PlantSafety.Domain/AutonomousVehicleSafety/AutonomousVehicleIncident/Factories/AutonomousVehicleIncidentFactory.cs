// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.AutonomousVehicleIncident
//  Layer:          Domain
//  File:           AutonomousVehicleIncidentFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing AutonomousVehicleIncident aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleIncident.Factories;

/// <summary>Default factory implementation for constructing <see cref="Aggregates.AutonomousVehicleIncident"/> aggregates.</summary>
public sealed class AutonomousVehicleIncidentFactory : IAutonomousVehicleIncidentFactory
{
    /// <inheritdoc/>
    public Aggregates.AutonomousVehicleIncident Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => Aggregates.AutonomousVehicleIncident.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}
