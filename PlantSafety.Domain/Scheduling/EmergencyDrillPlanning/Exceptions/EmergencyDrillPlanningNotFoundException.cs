// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.EmergencyDrillPlanning
//  Layer:          Domain
//  File:           EmergencyDrillPlanningNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a EmergencyDrillPlanning aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Exceptions;

/// <summary>Thrown when a <see cref="EmergencyDrillPlanning"/> aggregate lookup finds no match.</summary>
public sealed class EmergencyDrillPlanningNotFoundException : EmergencyDrillPlanningDomainException
{
    /// <summary>Creates a not-found exception for the given EmergencyDrillPlanning identity.</summary>
    public EmergencyDrillPlanningNotFoundException(EmergencyDrillPlanningId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"EmergencyDrillPlanning with id '{id}' was not found.",
            "EmergencyDrillPlanning.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}