// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.PredictiveAnalytics
//  Layer:          Domain
//  File:           PredictionConfidence.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 Predictive Model Integration)
//  SchemaVersion:  2026.1
//  Description:    Immutable value object representing the confidence (0.0-1.0) a predictive
//                  model attaches to a prediction, with a deterministic qualitative banding.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.PredictiveAnalytics.Predictors;

/// <summary>Qualitative confidence band derived from a <see cref="PredictionConfidence"/> value.</summary>
public enum PredictionConfidenceBand
{
    /// <summary>Confidence below 0.40 -- treat the prediction as indicative only.</summary>
    Low = 0,

    /// <summary>Confidence between 0.40 (inclusive) and 0.75 (exclusive).</summary>
    Medium = 1,

    /// <summary>Confidence at or above 0.75.</summary>
    High = 2,
}

/// <summary>A deterministic, bounded [0.0, 1.0] confidence score attached to a model prediction.</summary>
public readonly record struct PredictionConfidence
{
    /// <summary>The underlying confidence value, always within [0.0, 1.0].</summary>
    public double Value { get; }

    /// <summary>Creates a PredictionConfidence, clamping the supplied value into [0.0, 1.0].</summary>
    public PredictionConfidence(double value)
    {
        Value = Math.Clamp(value, 0.0, 1.0);
    }

    /// <summary>Qualitative band derived from <see cref="Value"/>.</summary>
    public PredictionConfidenceBand Band => Value switch
    {
        >= 0.75 => PredictionConfidenceBand.High,
        >= 0.40 => PredictionConfidenceBand.Medium,
        _ => PredictionConfidenceBand.Low,
    };

    /// <summary>Creates a PredictionConfidence from a raw sample count using a saturating curve (more samples = higher confidence).</summary>
    public static PredictionConfidence FromSampleCount(int sampleCount, int samplesForFullConfidence = 30)
    {
        if (samplesForFullConfidence < 1)
            throw new ArgumentOutOfRangeException(nameof(samplesForFullConfidence));

        var ratio = Math.Clamp((double)Math.Max(0, sampleCount) / samplesForFullConfidence, 0.0, 1.0);
        return new PredictionConfidence(ratio);
    }

    /// <inheritdoc/>
    public override string ToString() => $"{Value:P0} ({Band})";
}
