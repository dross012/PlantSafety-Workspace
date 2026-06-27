// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.ScoreCards
//  Layer:          Domain
//  File:           ScoreCardsEventId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.ScoreCards.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for ScoreCards domain events.</summary>
public readonly record struct ScoreCardsEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a ScoreCardsEventId wrapping a non-empty GUID.</summary>
    public ScoreCardsEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ScoreCardsEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new ScoreCardsEventId using Uuid7.</summary>
    public static ScoreCardsEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
