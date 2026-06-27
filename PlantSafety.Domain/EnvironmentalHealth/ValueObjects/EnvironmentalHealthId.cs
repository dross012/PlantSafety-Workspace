// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth
//  Layer:          Domain
//  File:           EnvironmentalHealthId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Strong-typed Uuid7-backed identity for the EnvironmentalHealth aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.ValueObjects;

/// <summary>Strong-typed, Uuid7-backed identity for the EnvironmentalHealth aggregate root.</summary>
public readonly record struct EnvironmentalHealthId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a EnvironmentalHealthId wrapping a non-empty GUID.</summary>
    public EnvironmentalHealthId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EnvironmentalHealthId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new EnvironmentalHealthId using Uuid7.</summary>
    public static EnvironmentalHealthId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a EnvironmentalHealthId.</summary>
    public static EnvironmentalHealthId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a EnvironmentalHealthId.</summary>
    public static EnvironmentalHealthId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
