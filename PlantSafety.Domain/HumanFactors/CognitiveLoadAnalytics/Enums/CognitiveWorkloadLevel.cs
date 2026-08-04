// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Domain
//  File:           CognitiveWorkloadLevel.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Classification of a single CognitiveLoadReading's measured workload score,
//                  evaluated by CognitiveOverloadPolicy. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Enums;

/// <summary>Classification of a single CognitiveLoadReading's measured workload score.</summary>
public enum CognitiveWorkloadLevel
{
    /// <summary>Cognitive load score is within the low-demand band.</summary>
    Low = 0,

    /// <summary>Cognitive load score is within the moderate-demand band.</summary>
    Moderate = 1,

    /// <summary>Cognitive load score is within the high-demand band.</summary>
    High = 2,

    /// <summary>Cognitive load score meets or exceeds the overload threshold. Triggers CognitiveOverloadDetectedEvent.</summary>
    Overload = 3,
}
