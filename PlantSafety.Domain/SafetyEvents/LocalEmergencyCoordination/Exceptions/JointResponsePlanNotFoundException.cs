// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Domain
//  File:           JointResponsePlanNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a JointResponsePlan child entity lookup within a
//                  LocalEmergencyCoordination aggregate finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Exceptions;

/// <summary>Thrown when a <see cref="Entities.JointResponsePlan"/> child entity lookup finds no match.</summary>
public sealed class JointResponsePlanNotFoundException : LocalEmergencyCoordinationDomainException
{
    /// <summary>Creates a not-found exception for the given JointResponsePlan identity.</summary>
    public JointResponsePlanNotFoundException(JointResponsePlanId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"JointResponsePlan with id '{id}' was not found on the LocalEmergencyCoordination aggregate.",
            "LocalEmergencyCoordination.JointResponsePlanNotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
