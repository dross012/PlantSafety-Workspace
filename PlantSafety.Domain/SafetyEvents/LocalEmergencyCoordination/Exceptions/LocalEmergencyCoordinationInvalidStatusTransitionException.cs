// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Domain
//  File:           LocalEmergencyCoordinationInvalidStatusTransitionException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a LocalEmergencyCoordination lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Exceptions;

/// <summary>Thrown when a <see cref="Aggregates.LocalEmergencyCoordination"/> lifecycle transition is not permitted.</summary>
public sealed class LocalEmergencyCoordinationInvalidStatusTransitionException : LocalEmergencyCoordinationDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public LocalEmergencyCoordinationInvalidStatusTransitionException(
        LocalEmergencyCoordinationId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'LocalEmergencyCoordination' {id} from '{from}' to '{to}'.",
            "LocalEmergencyCoordination.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
