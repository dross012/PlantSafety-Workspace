// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Domain
//  File:           ExternalResponderLiaisonNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when an ExternalResponderLiaison child entity lookup within a
//                  LocalEmergencyCoordination aggregate finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Exceptions;

/// <summary>Thrown when an <see cref="Entities.ExternalResponderLiaison"/> child entity lookup finds no match.</summary>
public sealed class ExternalResponderLiaisonNotFoundException : LocalEmergencyCoordinationDomainException
{
    /// <summary>Creates a not-found exception for the given ExternalResponderLiaison identity.</summary>
    public ExternalResponderLiaisonNotFoundException(ExternalResponderLiaisonId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"ExternalResponderLiaison with id '{id}' was not found on the LocalEmergencyCoordination aggregate.",
            "LocalEmergencyCoordination.ExternalResponderLiaisonNotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
