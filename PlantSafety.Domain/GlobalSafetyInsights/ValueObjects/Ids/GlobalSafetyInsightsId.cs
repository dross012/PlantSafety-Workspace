// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Domain
//  File:           GlobalSafetyInsightsId.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the GlobalSafetyInsightsAggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.GlobalSafetyInsights.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the GlobalSafetyInsightsAggregate root.</summary>
public readonly record struct GlobalSafetyInsightsId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a GlobalSafetyInsightsId wrapping a non-empty GUID.</summary>
    public GlobalSafetyInsightsId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("GlobalSafetyInsightsId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new GlobalSafetyInsightsId using Uuid7.</summary>
    public static GlobalSafetyInsightsId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a GlobalSafetyInsightsId.</summary>
    public static GlobalSafetyInsightsId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a GlobalSafetyInsightsId.</summary>
    public static GlobalSafetyInsightsId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
