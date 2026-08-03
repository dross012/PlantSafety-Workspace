// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Application
//  File:           PredictiveMaintenanceRecommendationFactory.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Builds the single canonical recommendation for a PredictiveMaintenanceAlert.
//                  Shared by GetPredictiveMaintenanceRecommendationsQueryHandler (list) and the
//                  Api-layer recommendation-detail action (which reuses GetPredictiveMaintenanceAlertByIdQuery
//                  directly rather than a second query, per PredictiveMaintenanceRecommendationDto's
//                  1:1-with-alert design). Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Operational.PredictiveMaintenance.Models;

namespace PlantSafety.Application.Operational.PredictiveMaintenance.Services;

/// <summary>Builds the canonical <see cref="PredictiveMaintenanceRecommendationDto"/> for a PredictiveMaintenanceAlert.</summary>
public static class PredictiveMaintenanceRecommendationFactory
{
    private static readonly IReadOnlyList<string> DefaultSteps =
    [
        "Review the predicted failure mode and confirm current equipment condition.",
        "Schedule corrective maintenance before the forecasted risk window.",
        "Verify sensor readings return to baseline after the corrective action is completed.",
    ];

    /// <summary>Builds the canonical recommendation for the given alert.</summary>
    public static PredictiveMaintenanceRecommendationDto Build(PredictiveMaintenanceAlertItemDto alert)
    {
        var bucket = PredictiveMaintenanceInsightsCalculator.ClassifyStatus(alert.Status);
        var priority = bucket switch
        {
            PredictiveMaintenanceRiskBucket.High => "Critical",
            PredictiveMaintenanceRiskBucket.Medium => "High",
            _ => "Low",
        };

        var estimatedRiskReduction = bucket switch
        {
            PredictiveMaintenanceRiskBucket.High => 0.45,
            PredictiveMaintenanceRiskBucket.Medium => 0.25,
            _ => 0.05,
        };

        return new PredictiveMaintenanceRecommendationDto
        {
            RecommendationId = alert.PredictiveMaintenanceAlertId,
            Title = $"Address predicted {alert.PredictedFailureType} on equipment {PredictiveMaintenanceInsightsCalculator.ShortCode("EQ", alert.EquipmentId)}",
            Priority = priority,
            Steps = DefaultSteps,
            EstimatedRiskReduction = estimatedRiskReduction,
        };
    }
}
