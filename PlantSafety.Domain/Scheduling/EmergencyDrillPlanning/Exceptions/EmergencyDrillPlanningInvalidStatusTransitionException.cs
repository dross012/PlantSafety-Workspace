// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.EmergencyDrillPlanning
//  Layer:          Domain
//  File:           EmergencyDrillPlanningInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a EmergencyDrillPlanning lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Exceptions;

/// <summary>Thrown when a <see cref="EmergencyDrillPlanning"/> lifecycle transition is not permitted.</summary>
public sealed class EmergencyDrillPlanningInvalidStatusTransitionException : EmergencyDrillPlanningDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public EmergencyDrillPlanningInvalidStatusTransitionException(
        EmergencyDrillPlanningId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'EmergencyDrillPlanning' {id} from '{from}' to '{to}'.",
            "EmergencyDrillPlanning.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}