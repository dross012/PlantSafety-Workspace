// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Domain
//  File:           CognitiveLoadAnalyticsEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for domain events raised across the
//                  CognitiveLoadAnalytics sub-module. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for CognitiveLoadAnalytics domain events.</summary>
public readonly record struct CognitiveLoadAnalyticsEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a CognitiveLoadAnalyticsEventId wrapping a non-empty GUID.</summary>
    public CognitiveLoadAnalyticsEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CognitiveLoadAnalyticsEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new CognitiveLoadAnalyticsEventId using Uuid7.</summary>
    public static CognitiveLoadAnalyticsEventId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a CognitiveLoadAnalyticsEventId.</summary>
    public static CognitiveLoadAnalyticsEventId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
