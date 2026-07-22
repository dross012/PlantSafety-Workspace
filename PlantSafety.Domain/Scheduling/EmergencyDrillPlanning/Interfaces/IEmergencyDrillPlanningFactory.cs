// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.EmergencyDrillPlanning
//  Layer:          Domain
//  File:           IEmergencyDrillPlanningFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing EmergencyDrillPlanning aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Interfaces;

/// <summary>Factory interface for constructing <see cref="EmergencyDrillPlanning"/> aggregates.</summary>
public interface IEmergencyDrillPlanningFactory
{
    /// <summary>Creates a new EmergencyDrillPlanning aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Aggregates.EmergencyDrillPlanning Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}