// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Domain
//  File:           CognitiveOverloadPolicy.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain policy classifying a single measured cognitive-load score into a
//                  CognitiveWorkloadLevel. Risk hook: the Overload classification is the trigger
//                  CognitiveLoadLedger.RecordCognitiveLoadReading uses to raise
//                  CognitiveOverloadDetectedEvent for RiskManagement consumption.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Policies;

/// <summary>Classifies a measured cognitive-load score (0-100) into a <see cref="CognitiveWorkloadLevel"/>.</summary>
public static class CognitiveOverloadPolicy
{
    /// <summary>Score at or above which a reading is classified as an overload event.</summary>
    public const decimal OverloadThreshold = 80m;

    /// <summary>Score at or above which a reading is classified as high (but not yet overload).</summary>
    public const decimal HighThreshold = 60m;

    /// <summary>Score at or above which a reading is classified as moderate.</summary>
    public const decimal ModerateThreshold = 35m;

    /// <summary>Classifies the given cognitive-load score.</summary>
    public static CognitiveWorkloadLevel Classify(decimal cognitiveLoadScore) => cognitiveLoadScore switch
    {
        >= OverloadThreshold => CognitiveWorkloadLevel.Overload,
        >= HighThreshold => CognitiveWorkloadLevel.High,
        >= ModerateThreshold => CognitiveWorkloadLevel.Moderate,
        _ => CognitiveWorkloadLevel.Low,
    };

    /// <summary>Returns <see langword="true"/> when the given score is classified as an overload event.</summary>
    public static bool IsOverload(decimal cognitiveLoadScore) => cognitiveLoadScore >= OverloadThreshold;
}
