// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AccidentManagement
//  Layer:          Domain
//  File:           CorrectiveActionNotFoundException.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a CorrectiveAction lookup within an Accident aggregate finds no match.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.Domain.AccidentManagement.ValueObjects.Ids;

namespace PlantSafety.Domain.AccidentManagement.Exceptions;

/// <summary>Thrown when a CorrectiveAction lookup within an Accident aggregate finds no match.</summary>
public sealed class CorrectiveActionNotFoundException : AccidentDomainException
{
    /// <summary>Creates a not-found exception for the given CorrectiveAction identity.</summary>
    public CorrectiveActionNotFoundException(AccidentId accidentId, CorrectiveActionId correctiveActionId, CorrelationId correlationId, CausationId causationId)
        : base(
            $"CorrectiveAction '{correctiveActionId}' was not found on Accident '{accidentId}'.",
            "Accident.CorrectiveActionNotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
