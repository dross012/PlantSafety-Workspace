// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Domain
//  File:           CognitiveRiskPredictionStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the CognitiveRiskPrediction aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Enums;

/// <summary>Lifecycle states for the CognitiveRiskPrediction aggregate root.</summary>
public enum CognitiveRiskPredictionStatus
{
    /// <summary>Prediction has been generated and is awaiting acknowledgement/response.</summary>
    Generated = 0,

    /// <summary>Prediction (and, where applicable, its alert) has been acknowledged by a responder.</summary>
    Acknowledged = 1,

    /// <summary>Prediction was reviewed and dismissed as not actionable.</summary>
    Dismissed = 2,
}
