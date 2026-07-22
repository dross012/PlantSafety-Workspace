// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scorecards
//  Layer:          Domain
//  File:           ScorecardNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a Scorecard aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.Domain.Scorecards.ValueObjects.Ids;

namespace PlantSafety.Domain.Scorecards.Exceptions;

/// <summary>Thrown when a Scorecard aggregate lookup finds no match.</summary>
public sealed class ScorecardNotFoundException : ScorecardDomainException
{
    /// <summary>Creates a not-found exception for the given Scorecard identity.</summary>
    public ScorecardNotFoundException(ScorecardId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"Scorecard with id '{id}' was not found.",
            "Scorecard.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}