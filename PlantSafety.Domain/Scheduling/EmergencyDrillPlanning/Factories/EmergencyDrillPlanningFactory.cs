// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.EmergencyDrillPlanning
//  Layer:          Domain
//  File:           EmergencyDrillPlanningFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing EmergencyDrillPlanning aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Factories;

/// <summary>Default factory implementation for constructing <see cref="EmergencyDrillPlanning"/> aggregates.</summary>
public sealed class EmergencyDrillPlanningFactory : IEmergencyDrillPlanningFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Aggregates.EmergencyDrillPlanning Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Aggregates.EmergencyDrillPlanning.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}