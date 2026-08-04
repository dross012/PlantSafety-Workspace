// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Domain
//  File:           CognitiveRiskPredictionId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the CognitiveRiskPrediction aggregate
//                  root. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the CognitiveRiskPrediction aggregate root.</summary>
public readonly record struct CognitiveRiskPredictionId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a CognitiveRiskPredictionId wrapping a non-empty GUID.</summary>
    public CognitiveRiskPredictionId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CognitiveRiskPredictionId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new CognitiveRiskPredictionId using Uuid7.</summary>
    public static CognitiveRiskPredictionId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a CognitiveRiskPredictionId.</summary>
    public static CognitiveRiskPredictionId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a CognitiveRiskPredictionId.</summary>
    public static CognitiveRiskPredictionId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
