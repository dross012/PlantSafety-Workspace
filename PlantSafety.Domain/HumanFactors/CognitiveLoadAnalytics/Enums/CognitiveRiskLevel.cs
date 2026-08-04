// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Domain
//  File:           CognitiveRiskLevel.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Predicted cognitive-risk classification produced by CognitiveRiskPredictionPolicy.
//                  Elevated/Critical predictions are risk hooks consumed by RiskManagement and
//                  correlated against AccidentManagement root-cause data. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Enums;

/// <summary>Predicted cognitive-risk classification for a CognitiveRiskPrediction.</summary>
public enum CognitiveRiskLevel
{
    /// <summary>Low predicted risk of cognitive-overload-related incident.</summary>
    Low = 0,

    /// <summary>Moderate predicted risk; worth routine monitoring.</summary>
    Moderate = 1,

    /// <summary>Elevated predicted risk; alert-worthy (RiskManagement risk hook).</summary>
    Elevated = 2,

    /// <summary>Critical predicted risk; immediate acknowledgement/response expected.</summary>
    Critical = 3,
}
