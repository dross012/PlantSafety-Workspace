// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scorecards
//  Layer:          Domain
//  File:           ScorecardFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing Scorecard aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Scorecards.Aggregates;
using PlantSafety.Domain.Scorecards.Interfaces;

namespace PlantSafety.Domain.Scorecards.Factories;

/// <summary>Default factory implementation for constructing Scorecard aggregates.</summary>
public sealed class ScorecardFactory : IScorecardFactory
{
    /// <inheritdoc/>
    public Scorecard Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => Scorecard.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}