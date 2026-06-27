// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.ScoreCards
//  Layer:          Domain
//  File:           ScoreCardsId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.ScoreCards.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the ScoreCards aggregate root.</summary>
public readonly record struct ScoreCardsId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a ScoreCardsId wrapping a non-empty GUID.</summary>
    public ScoreCardsId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ScoreCardsId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new ScoreCardsId using Uuid7.</summary>
    public static ScoreCardsId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a ScoreCardsId.</summary>
    public static ScoreCardsId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a ScoreCardsId.</summary>
    public static ScoreCardsId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
