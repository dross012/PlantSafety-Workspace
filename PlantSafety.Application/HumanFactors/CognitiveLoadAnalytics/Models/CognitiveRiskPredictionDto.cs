// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Application
//  File:           CognitiveRiskPredictionDto.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Read-model DTO backing ListElevatedCognitiveRiskPredictionsQuery. Backs the
//                  Predictive Cognitive Risk Board dashboard and the RiskManagement/
//                  AccidentManagement risk-hook correlation feed. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Models;

/// <summary>A single cognitive-risk prediction read-model record.</summary>
public sealed record CognitiveRiskPredictionDto
{
    /// <summary>Prediction identifier.</summary>
    public required Guid CognitiveRiskPredictionId { get; init; }

    /// <summary>Worker identifier this prediction was generated for.</summary>
    public required Guid WorkerUserId { get; init; }

    /// <summary>Source ledger identifier.</summary>
    public required Guid SourceLedgerId { get; init; }

    /// <summary>Predicted cognitive-risk classification.</summary>
    public required string PredictedRiskLevel { get; init; }

    /// <summary>Normalized (0-1) prediction confidence score.</summary>
    public decimal PredictionScore { get; init; }

    /// <summary>Predictive model version used.</summary>
    public string ModelVersion { get; init; } = string.Empty;

    /// <summary>Current lifecycle status (Generated/Acknowledged/Dismissed).</summary>
    public required string Status { get; init; }

    /// <summary>UTC instant the prediction was generated.</summary>
    public DateTime GeneratedAtUtc { get; init; }

    /// <summary>UTC instant the alert was acknowledged, if it has been.</summary>
    public DateTime? AcknowledgedAtUtc { get; init; }
}
