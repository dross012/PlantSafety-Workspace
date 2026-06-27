// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Culture
//  Layer:          Domain
//  File:           CultureId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Culture.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the Culture aggregate root.</summary>
public readonly record struct CultureId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a CultureId wrapping a non-empty GUID.</summary>
    public CultureId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CultureId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new CultureId using Uuid7.</summary>
    public static CultureId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a CultureId.</summary>
    public static CultureId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a CultureId.</summary>
    public static CultureId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
