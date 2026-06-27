// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors
//  Layer:          Domain
//  File:           HumanFactorsId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Strong-typed Uuid7-backed identity for the HumanFactors aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.ValueObjects;

/// <summary>Strong-typed, Uuid7-backed identity for the HumanFactors aggregate root.</summary>
public readonly record struct HumanFactorsId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a HumanFactorsId wrapping a non-empty GUID.</summary>
    public HumanFactorsId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("HumanFactorsId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new HumanFactorsId using Uuid7.</summary>
    public static HumanFactorsId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a HumanFactorsId.</summary>
    public static HumanFactorsId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a HumanFactorsId.</summary>
    public static HumanFactorsId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
