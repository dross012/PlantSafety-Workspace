// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Domain
//  File:           GlobalSafetyInsightsEventId.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for GlobalSafetyInsights domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.GlobalSafetyInsights.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for GlobalSafetyInsights domain events.</summary>
public readonly record struct GlobalSafetyInsightsEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a GlobalSafetyInsightsEventId wrapping a non-empty GUID.</summary>
    public GlobalSafetyInsightsEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("GlobalSafetyInsightsEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new GlobalSafetyInsightsEventId using Uuid7.</summary>
    public static GlobalSafetyInsightsEventId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a GlobalSafetyInsightsEventId.</summary>
    public static GlobalSafetyInsightsEventId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
