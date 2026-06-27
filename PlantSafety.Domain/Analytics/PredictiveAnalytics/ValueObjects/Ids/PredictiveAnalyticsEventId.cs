// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.PredictiveAnalytics
//  Layer:          Domain
//  File:           PredictiveAnalyticsEventId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.PredictiveAnalytics.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for PredictiveAnalytics domain events.</summary>
public readonly record struct PredictiveAnalyticsEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PredictiveAnalyticsEventId wrapping a non-empty GUID.</summary>
    public PredictiveAnalyticsEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PredictiveAnalyticsEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PredictiveAnalyticsEventId using Uuid7.</summary>
    public static PredictiveAnalyticsEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
