// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContinuousImprovement.CAPA
//  Layer:          Domain
//  File:           CorrectiveActionNotFoundException.cs
//  Created:        2026-08-06
//  SchemaVersion:  2026.1
//  Description:    Thrown when a CorrectiveAction aggregate lookup finds no match. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.Domain.ContinuousImprovement.CAPA.ValueObjects.Ids;

namespace PlantSafety.Domain.ContinuousImprovement.CAPA.Exceptions;

/// <summary>Thrown when a <see cref="Aggregates.CorrectiveAction"/> aggregate lookup finds no match.</summary>
public sealed class CorrectiveActionNotFoundException : CorrectiveActionDomainException
{
    /// <summary>Creates a not-found exception for the given CorrectiveAction identity.</summary>
    public CorrectiveActionNotFoundException(CorrectiveActionId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"CorrectiveAction with id '{id}' was not found.",
            "CorrectiveAction.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
