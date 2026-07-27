// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContinuousImprovement.CAPA
//  Layer:          Domain
//  File:           CorrectiveActionInvalidStatusTransitionException.cs
//  Created:        2026-07-27
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a CorrectiveAction lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.Domain.ContinuousImprovement.CAPA.ValueObjects.Ids;

namespace PlantSafety.Domain.ContinuousImprovement.CAPA.Exceptions;

/// <summary>Thrown when a <see cref="Aggregates.CorrectiveAction"/> lifecycle transition is not permitted.</summary>
public sealed class CorrectiveActionInvalidStatusTransitionException : CorrectiveActionDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public CorrectiveActionInvalidStatusTransitionException(
        CorrectiveActionId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition CorrectiveAction '{id}' from '{from}' to '{to}'.",
            "CorrectiveAction.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
