// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContinuousImprovement.RCA
//  Layer:          Domain
//  File:           RCAInvalidStatusTransitionException.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when an RCA lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.ContinuousImprovement.RCA.ValueObjects.Ids;

namespace PlantSafety.Domain.ContinuousImprovement.RCA.Exceptions;

/// <summary>Thrown when an RCA lifecycle transition is not permitted.</summary>
public sealed class RCAInvalidStatusTransitionException : RCADomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public RCAInvalidStatusTransitionException(
        RCAId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition RCA '{id}' from '{from}' to '{to}'.",
            "RCA.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
