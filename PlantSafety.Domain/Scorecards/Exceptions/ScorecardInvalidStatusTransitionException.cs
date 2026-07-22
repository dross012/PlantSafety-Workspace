// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scorecards
//  Layer:          Domain
//  File:           ScorecardInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a Scorecard lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.Domain.Scorecards.ValueObjects.Ids;

namespace PlantSafety.Domain.Scorecards.Exceptions;

/// <summary>Thrown when a Scorecard lifecycle transition is not permitted.</summary>
public sealed class ScorecardInvalidStatusTransitionException : ScorecardDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public ScorecardInvalidStatusTransitionException(
        ScorecardId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'Scorecard' {id} from '{from}' to '{to}'.",
            "Scorecard.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}