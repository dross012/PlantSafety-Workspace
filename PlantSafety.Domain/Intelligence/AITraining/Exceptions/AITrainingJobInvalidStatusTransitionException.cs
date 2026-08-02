// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AITraining
//  Layer:          Domain
//  File:           AITrainingJobInvalidStatusTransitionException.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when an AITrainingJob lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Intelligence.AITraining.ValueObjects.Ids;

namespace PlantSafety.Domain.Intelligence.AITraining.Exceptions;

/// <summary>Thrown when an AITrainingJob lifecycle transition is not permitted.</summary>
public sealed class AITrainingJobInvalidStatusTransitionException : AITrainingJobDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public AITrainingJobInvalidStatusTransitionException(
        AITrainingJobId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition AITrainingJob '{id}' from '{from}' to '{to}'.",
            "AITraining.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
