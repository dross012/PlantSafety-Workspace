// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.EmergencyResponsePlanning
//  Layer:          Domain
//  File:           EmergencyResponsePlanningFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing EmergencyResponsePlanning aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.EmergencyResponsePlanning.Factories;

/// <summary>Default factory implementation for constructing <see cref="EmergencyResponsePlanning"/> aggregates.</summary>
public sealed class EmergencyResponsePlanningFactory : IEmergencyResponsePlanningFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.SafetyProcedures.EmergencyResponsePlanning.Aggregates.EmergencyResponsePlanning Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.SafetyProcedures.EmergencyResponsePlanning.Aggregates.EmergencyResponsePlanning.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}