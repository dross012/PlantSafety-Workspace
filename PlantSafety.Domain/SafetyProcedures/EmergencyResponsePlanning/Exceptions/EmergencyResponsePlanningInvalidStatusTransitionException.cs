// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.EmergencyResponsePlanning
//  Layer:          Domain
//  File:           EmergencyResponsePlanningInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a EmergencyResponsePlanning lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.EmergencyResponsePlanning.Exceptions;

/// <summary>Thrown when a <see cref="EmergencyResponsePlanning"/> lifecycle transition is not permitted.</summary>
public sealed class EmergencyResponsePlanningInvalidStatusTransitionException : EmergencyResponsePlanningDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public EmergencyResponsePlanningInvalidStatusTransitionException(
        EmergencyResponsePlanningId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            "Cannot transition 'EmergencyResponsePlanning' {id} from '{from}' to '{to}'.",
            "EmergencyResponsePlanning.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}