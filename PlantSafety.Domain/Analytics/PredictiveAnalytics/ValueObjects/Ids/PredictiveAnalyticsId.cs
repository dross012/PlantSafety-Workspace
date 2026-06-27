// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.PredictiveAnalytics
//  Layer:          Domain
//  File:           PredictiveAnalyticsId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.PredictiveAnalytics.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the PredictiveAnalytics aggregate root.</summary>
public readonly record struct PredictiveAnalyticsId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PredictiveAnalyticsId wrapping a non-empty GUID.</summary>
    public PredictiveAnalyticsId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PredictiveAnalyticsId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PredictiveAnalyticsId using Uuid7.</summary>
    public static PredictiveAnalyticsId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a PredictiveAnalyticsId.</summary>
    public static PredictiveAnalyticsId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a PredictiveAnalyticsId.</summary>
    public static PredictiveAnalyticsId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
