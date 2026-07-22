// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.EmergencyResponsePlanning
//  Layer:          Domain
//  File:           EmergencyResponsePlanningNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a EmergencyResponsePlanning aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.EmergencyResponsePlanning.Exceptions;

/// <summary>Thrown when a <see cref="EmergencyResponsePlanning"/> aggregate lookup finds no match.</summary>
public sealed class EmergencyResponsePlanningNotFoundException : EmergencyResponsePlanningDomainException
{
    /// <summary>Creates a not-found exception for the given EmergencyResponsePlanning identity.</summary>
    public EmergencyResponsePlanningNotFoundException(EmergencyResponsePlanningId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "EmergencyResponsePlanning with id '{id}' was not found.",
            "EmergencyResponsePlanning.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}