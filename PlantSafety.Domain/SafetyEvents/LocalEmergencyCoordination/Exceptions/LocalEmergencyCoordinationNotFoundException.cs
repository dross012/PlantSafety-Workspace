// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Domain
//  File:           LocalEmergencyCoordinationNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a LocalEmergencyCoordination aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Exceptions;

/// <summary>Thrown when a <see cref="Aggregates.LocalEmergencyCoordination"/> aggregate lookup finds no match.</summary>
public sealed class LocalEmergencyCoordinationNotFoundException : LocalEmergencyCoordinationDomainException
{
    /// <summary>Creates a not-found exception for the given LocalEmergencyCoordination identity.</summary>
    public LocalEmergencyCoordinationNotFoundException(LocalEmergencyCoordinationId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"LocalEmergencyCoordination with id '{id}' was not found.",
            "LocalEmergencyCoordination.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
