// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.ScoreCards
//  Layer:          Application
//  File:           CreateScoreCardsResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Result payload for CreateScoreCardsCommand.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Analytics.ScoreCards.Commands;

/// <summary>Result of a successful <see cref="CreateScoreCardsCommand"/> execution.</summary>
/// <param name="ScoreCardsId">Identity of the newly created ScoreCards aggregate.</param>
/// <param name="Status">Lifecycle status of the newly created record.</param>
public sealed record CreateScoreCardsResult(
    Guid ScoreCardsId,
    string Status);
