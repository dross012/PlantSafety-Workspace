// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scorecards
//  Layer:          Domain
//  File:           IScorecardFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing Scorecard aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Scorecards.Aggregates;

namespace PlantSafety.Domain.Scorecards.Interfaces;

/// <summary>Factory interface for constructing Scorecard aggregates.</summary>
public interface IScorecardFactory
{
    /// <summary>Creates a new Scorecard aggregate with the supplied parameters.</summary>
    Scorecard Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}