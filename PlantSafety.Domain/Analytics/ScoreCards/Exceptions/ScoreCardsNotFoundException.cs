// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.ScoreCards
//  Layer:          Domain
//  File:           ScoreCardsNotFoundException.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.ScoreCards.Exceptions;

/// <summary>Thrown when a <see cref="ScoreCards"/> aggregate lookup finds no match.</summary>
public sealed class ScoreCardsNotFoundException : ScoreCardsDomainException
{
    /// <summary>Creates a not-found exception for the given ScoreCards identity.</summary>
    public ScoreCardsNotFoundException(ScoreCardsId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "ScoreCards with id '{id}' was not found.",
            "ScoreCards.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
