// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Application
//  File:           PredictiveMaintenanceInsightsCalculator.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Shared derivation logic for the "Predictive Maintenance Insights" query
//                  handlers: risk-bucket classification of a PredictiveMaintenanceAlert's
//                  lifecycle status, and the real FailureLikelihoodPredictor invocation used to
//                  compute a genuine risk score/confidence for a given alert. Centralised here so
//                  every handler (risk-summary, equipment-health, predictions, equipment view,
//                  prediction summary/explanation/forecast) derives risk identically instead of
//                  re-implementing the same status->bucket mapping. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Operational.PredictiveMaintenance.Models;
using PlantSafety.Domain.Analytics.PredictiveAnalytics.Predictors;
using PlantSafety.Domain.Intelligence.PredictiveSafety.Predictors;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Application.Operational.PredictiveMaintenance.Services;

/// <summary>Qualitative risk bucket derived from a PredictiveMaintenanceAlert's lifecycle status.</summary>
public enum PredictiveMaintenanceRiskBucket
{
    /// <summary>Alert still requires triage/action (Raised, UnderReview).</summary>
    High = 0,

    /// <summary>Corrective action has been scheduled but not yet completed (ActionScheduled).</summary>
    Medium = 1,

    /// <summary>Alert reached a terminal, addressed status (Resolved, Dismissed, Closed).</summary>
    Low = 2,
}

/// <summary>Derives risk buckets and real predictor-backed risk scores for PredictiveMaintenanceAlert records.</summary>
public sealed class PredictiveMaintenanceInsightsCalculator
{
    private readonly FailureLikelihoodPredictor _predictor = new();

    /// <summary>The underlying real predictor instance, exposed so callers can reuse its ModelName/ModelVersion/weights.</summary>
    public FailureLikelihoodPredictor Predictor => _predictor;

    /// <summary>Classifies a lifecycle status string into a risk bucket.</summary>
    public static PredictiveMaintenanceRiskBucket ClassifyStatus(string status) => status switch
    {
        "Raised" or "UnderReview" => PredictiveMaintenanceRiskBucket.High,
        "ActionScheduled" => PredictiveMaintenanceRiskBucket.Medium,
        "Resolved" or "Dismissed" or "Closed" => PredictiveMaintenanceRiskBucket.Low,
        _ => PredictiveMaintenanceRiskBucket.Medium,
    };

    /// <summary>Maps a risk bucket to its display string.</summary>
    public static string BucketLabel(PredictiveMaintenanceRiskBucket bucket) => bucket switch
    {
        PredictiveMaintenanceRiskBucket.High => "High",
        PredictiveMaintenanceRiskBucket.Medium => "Medium",
        _ => "Low",
    };

    /// <summary>Maps a risk bucket to an equipment-health display string (Critical/Warning/Healthy).</summary>
    public static string HealthLabel(PredictiveMaintenanceRiskBucket bucket) => bucket switch
    {
        PredictiveMaintenanceRiskBucket.High => "Critical",
        PredictiveMaintenanceRiskBucket.Medium => "Warning",
        _ => "Healthy",
    };

    /// <summary>
    /// Runs the real FailureLikelihoodPredictor against inputs derived from an alert's real
    /// fields: elapsed hours since prediction (RunHoursSinceLastPm proxy), a fixed 30-day
    /// expected PM interval, and a status-derived anomaly score (higher for still-open alerts).
    /// Equipment age and historical-failure-count inputs are not yet sourced from a real
    /// equipment master/history feed, so fixed platform-default values are used and documented
    /// here -- flagged as an assumption pending that data source.
    /// </summary>
    public PredictionResult<FailureLikelihoodPredictionOutput> PredictForAlert(
        string status, DateTime predictedAtUtc, DateTime asOfUtc)
    {
        var bucket = ClassifyStatus(status);
        var elapsedHours = Math.Max(0.0, (asOfUtc - predictedAtUtc).TotalHours);

        var anomalyScore = bucket switch
        {
            PredictiveMaintenanceRiskBucket.High => 0.7,
            PredictiveMaintenanceRiskBucket.Medium => 0.4,
            _ => 0.1,
        };

        var input = new FailureLikelihoodPredictionInput
        {
            EquipmentAgeMonths = 24,
            RunHoursSinceLastPm = elapsedHours,
            ExpectedPmIntervalHours = 720.0,
            HistoricalFailureCount90Days = bucket == PredictiveMaintenanceRiskBucket.Low ? 0 : 1,
            AnomalyScore = anomalyScore,
        };

        return _predictor.Predict(input, UtcTimestamp.From(DateTime.SpecifyKind(asOfUtc, DateTimeKind.Utc)));
    }

    /// <summary>Builds the real feature-importance breakdown from the predictor's public blend weights.</summary>
    public static IReadOnlyList<PredictiveMaintenanceFeatureImportanceDto> BuildFeatureImportance() =>
    [
        new PredictiveMaintenanceFeatureImportanceDto
        {
            FeatureName = "PM Interval Overrun",
            Contribution = FailureLikelihoodPredictor.PmOverrunWeight,
            IsPositiveInfluence = true,
        },
        new PredictiveMaintenanceFeatureImportanceDto
        {
            FeatureName = "Equipment Age",
            Contribution = FailureLikelihoodPredictor.AgeWeight,
            IsPositiveInfluence = true,
        },
        new PredictiveMaintenanceFeatureImportanceDto
        {
            FeatureName = "Recent Failure History (90d)",
            Contribution = FailureLikelihoodPredictor.HistoricalFailureWeight,
            IsPositiveInfluence = true,
        },
        new PredictiveMaintenanceFeatureImportanceDto
        {
            FeatureName = "Sensor Anomaly Score",
            Contribution = FailureLikelihoodPredictor.AnomalyWeight,
            IsPositiveInfluence = true,
        },
    ];

    /// <summary>Formats a short, human-readable prediction/equipment code from a GUID.</summary>
    public static string ShortCode(string prefix, Guid id) => $"{prefix}-{id.ToString("N")[..6].ToUpperInvariant()}";
}
