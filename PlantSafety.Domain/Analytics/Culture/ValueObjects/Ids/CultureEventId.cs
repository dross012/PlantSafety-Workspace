// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Culture
//  Layer:          Domain
//  File:           CultureEventId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Culture.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for Culture domain events.</summary>
public readonly record struct CultureEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a CultureEventId wrapping a non-empty GUID.</summary>
    public CultureEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CultureEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new CultureEventId using Uuid7.</summary>
    public static CultureEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
