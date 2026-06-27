// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.ScoreCards
//  Layer:          Domain
//  File:           IScoreCardsFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing ScoreCards aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.ScoreCards.Interfaces;

/// <summary>Factory interface for constructing <see cref="ScoreCards"/> aggregates.</summary>
public interface IScoreCardsFactory
{
    /// <summary>Creates a new ScoreCards aggregate with the supplied parameters.</summary>
    ScoreCards Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}
