// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Domain
//  File:           CognitiveLoadReadingId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the CognitiveLoadReading child entity.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the CognitiveLoadReading child entity.</summary>
public readonly record struct CognitiveLoadReadingId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a CognitiveLoadReadingId wrapping a non-empty GUID.</summary>
    public CognitiveLoadReadingId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CognitiveLoadReadingId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new CognitiveLoadReadingId using Uuid7.</summary>
    public static CognitiveLoadReadingId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a CognitiveLoadReadingId.</summary>
    public static CognitiveLoadReadingId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a CognitiveLoadReadingId.</summary>
    public static CognitiveLoadReadingId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
