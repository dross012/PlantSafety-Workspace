// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.ScoreCards
//  Layer:          Domain
//  File:           ScoreCardsFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing ScoreCards aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.ScoreCards.Factories;

/// <summary>Default factory implementation for constructing <see cref="ScoreCards"/> aggregates.</summary>
public sealed class ScoreCardsFactory : IScoreCardsFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.Analytics.ScoreCards.Aggregates.ScoreCards Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.Analytics.ScoreCards.Aggregates.ScoreCards.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}
