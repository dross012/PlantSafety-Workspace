// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability
//  Layer:          Domain
//  File:           SustainabilityId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Strong-typed Uuid7-backed identity for the Sustainability aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.ValueObjects;

/// <summary>Strong-typed, Uuid7-backed identity for the Sustainability aggregate root.</summary>
public readonly record struct SustainabilityId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a SustainabilityId wrapping a non-empty GUID.</summary>
    public SustainabilityId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SustainabilityId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new SustainabilityId using Uuid7.</summary>
    public static SustainabilityId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a SustainabilityId.</summary>
    public static SustainabilityId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a SustainabilityId.</summary>
    public static SustainabilityId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
