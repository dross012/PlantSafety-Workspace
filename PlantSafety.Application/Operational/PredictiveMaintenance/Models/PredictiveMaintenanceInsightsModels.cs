// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Application
//  File:           PredictiveMaintenanceInsightsModels.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Application-layer read models backing the 13 "Predictive Maintenance
//                  Insights" sub-resources (risk-summary, equipment-health, predictions,
//                  equipment predictive view, prediction summary/explanation/forecast/
//                  recommendations, recommendation detail, sensor data + anomalies,
//                  predictive reports). Every record here is derived from the real
//                  PredictiveMaintenanceAlert read-model (Operational.PredictiveMaintenance)
//                  plus, where noted, the real Phase 5 predictive-model/telemetry domain
//                  services (Intelligence.PredictiveSafety.Predictors.FailureLikelihoodPredictor,
//                  Observability.Telemetry.Services.TelemetryAnomalyDetector) -- not a second,
//                  parallel "predictive maintenance" domain. See docs/Phase5/ui-gaps-scoping.md
//                  §4 for the reuse-vs-new-build rationale behind each shape.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Operational.PredictiveMaintenance.Models;

/// <summary>Risk Summary card counts (High/Medium/Low), derived from PredictiveMaintenanceAlert status buckets.</summary>
public sealed record PredictiveMaintenanceRiskSummaryDto
{
    /// <summary>Count of alerts bucketed as high risk (Raised/UnderReview).</summary>
    public required int HighRiskCount { get; init; }

    /// <summary>Count of alerts bucketed as medium risk (ActionScheduled).</summary>
    public required int MediumRiskCount { get; init; }

    /// <summary>Count of alerts bucketed as low risk (Resolved/Dismissed/Closed).</summary>
    public required int LowRiskCount { get; init; }

    /// <summary>Trend direction of newly-raised alerts (Increasing/Decreasing/Stable) comparing the trailing two 7-day windows.</summary>
    public required string TrendDirection { get; init; }
}

/// <summary>Equipment Health card percentages (Healthy/Warning/Critical), derived from each equipment's most recent alert status.</summary>
public sealed record PredictiveMaintenanceEquipmentHealthDto
{
    /// <summary>Percentage of alert-tracked equipment currently healthy (latest alert Resolved/Closed, or no alert history).</summary>
    public required double HealthyPercent { get; init; }

    /// <summary>Percentage of alert-tracked equipment currently in a warning state (latest alert ActionScheduled/Dismissed).</summary>
    public required double WarningPercent { get; init; }

    /// <summary>Percentage of alert-tracked equipment currently critical (latest alert Raised/UnderReview).</summary>
    public required double CriticalPercent { get; init; }
}

/// <summary>Single row in the Predictions list, one per PredictiveMaintenanceAlert.</summary>
public sealed record PredictiveMaintenancePredictionRowDto
{
    /// <summary>The underlying PredictiveMaintenanceAlert identifier.</summary>
    public required Guid PredictionId { get; init; }

    /// <summary>Human-readable prediction code (format P-######, derived from the alert identifier).</summary>
    public required string PredictionCode { get; init; }

    /// <summary>Target equipment identifier.</summary>
    public required Guid EquipmentId { get; init; }

    /// <summary>Human-readable equipment code (format EQ-######, derived from the equipment identifier).</summary>
    public required string EquipmentCode { get; init; }

    /// <summary>Risk level bucket (High, Medium, Low) derived from the alert's lifecycle status.</summary>
    public required string RiskLevel { get; init; }

    /// <summary>
    /// Model confidence percentage (0-100), computed by running the real
    /// <c>Intelligence.PredictiveSafety.Predictors.FailureLikelihoodPredictor</c> against inputs
    /// derived from the alert's real fields (elapsed time since prediction, lifecycle status).
    /// </summary>
    public required double ConfidencePercent { get; init; }

    /// <summary>UTC timestamp this alert was last actioned.</summary>
    public required DateTime UpdatedAtUtc { get; init; }

    /// <summary>True when the alert has not yet reached a terminal status (Resolved/Dismissed/Closed).</summary>
    public required bool IsActive { get; init; }
}

/// <summary>Prediction Summary header, sourced directly from a single PredictiveMaintenanceAlert record.</summary>
public sealed record PredictiveMaintenancePredictionSummaryDto
{
    /// <summary>The underlying PredictiveMaintenanceAlert identifier.</summary>
    public required Guid PredictionId { get; init; }

    /// <summary>Human-readable prediction code.</summary>
    public required string PredictionCode { get; init; }

    /// <summary>Target equipment identifier.</summary>
    public required Guid EquipmentId { get; init; }

    /// <summary>Human-readable equipment code.</summary>
    public required string EquipmentCode { get; init; }

    /// <summary>Normalized risk score (0-1), the FailureLikelihoodPredictor output for this alert.</summary>
    public required double RiskScore { get; init; }

    /// <summary>Model confidence percentage (0-100).</summary>
    public required double ConfidencePercent { get; init; }

    /// <summary>UTC timestamp this alert was last actioned.</summary>
    public required DateTime UpdatedAtUtc { get; init; }

    /// <summary>Human-readable data freshness description derived from the alert's most recent actioned timestamp.</summary>
    public required string DataFreshness { get; init; }
}

/// <summary>Single ranked contributing factor, sourced from the real FailureLikelihoodPredictor blend weights.</summary>
public sealed record PredictiveMaintenanceFeatureImportanceDto
{
    /// <summary>Feature/variable name.</summary>
    public required string FeatureName { get; init; }

    /// <summary>Normalized contribution (the real model weight; sum across all factors = 1.0).</summary>
    public required double Contribution { get; init; }

    /// <summary>True for every factor in the current heuristic blend -- all four inputs increase risk as they increase.</summary>
    public required bool IsPositiveInfluence { get; init; }
}

/// <summary>
/// Model explanation panel data. Feature weights are the real, public
/// <c>FailureLikelihoodPredictor</c> blend weights, not fabricated figures; the explanation
/// summary is the real <c>PredictionResult.Rationale</c> text produced by running the model
/// against this alert's derived inputs.
/// </summary>
public sealed record PredictiveMaintenanceModelExplanationDto
{
    /// <summary>Ranked contributing factors (the model's four real blend weights).</summary>
    public required IReadOnlyList<PredictiveMaintenanceFeatureImportanceDto> Factors { get; init; }

    /// <summary>Traceable model version identifier (FailureLikelihoodPredictor.ModelVersion).</summary>
    public required string ModelVersion { get; init; }

    /// <summary>Plain-language explanation summary (the model's real Rationale output).</summary>
    public required string ExplanationSummary { get; init; }
}

/// <summary>Single point on the risk forecast curve, each independently computed by the real FailureLikelihoodPredictor.</summary>
public sealed record PredictiveMaintenanceForecastPointDto
{
    /// <summary>Forecast timestamp (UTC).</summary>
    public required DateTime TimestampUtc { get; init; }

    /// <summary>Forecasted risk score (0-1) at this timestamp.</summary>
    public required double RiskScore { get; init; }

    /// <summary>Lower confidence interval bound.</summary>
    public required double ConfidenceLower { get; init; }

    /// <summary>Upper confidence interval bound.</summary>
    public required double ConfidenceUpper { get; init; }
}

/// <summary>
/// A single recommended corrective action for a PredictiveMaintenanceAlert. Deliberately modeled
/// 1:1 with the alert (RecommendationId == PredictiveMaintenanceAlertId) rather than introducing
/// a new persisted sub-aggregate: there is exactly one canonical recommendation per alert today,
/// templated from the alert's failure type and risk bucket. This keeps recommendation-detail
/// lookup a direct, honest read of the same alert record instead of inventing storage for data
/// that does not otherwise exist. See docs/Phase5/ui-gaps-scoping.md §4.
/// </summary>
public sealed record PredictiveMaintenanceRecommendationDto
{
    /// <summary>Recommendation identifier -- always equal to the source PredictiveMaintenanceAlertId.</summary>
    public required Guid RecommendationId { get; init; }

    /// <summary>Recommendation title, templated from the alert's predicted failure type.</summary>
    public required string Title { get; init; }

    /// <summary>Priority (Low, Medium, High, Critical), derived from the alert's risk bucket.</summary>
    public required string Priority { get; init; }

    /// <summary>Ordered step-by-step guidance.</summary>
    public required IReadOnlyList<string> Steps { get; init; }

    /// <summary>
    /// Estimated risk reduction (0-1) if followed -- the delta between the alert's current
    /// FailureLikelihoodPredictor score and the same model re-run with a zeroed anomaly/overrun
    /// signal (i.e. "if the recommended action is taken").
    /// </summary>
    public required double EstimatedRiskReduction { get; init; }
}

/// <summary>
/// A single sensor-channel time-series point. Values are deterministically synthesized from the
/// alert's real EquipmentId/PredictedAtUtc (seeded, not random) using the real
/// Observability.Telemetry value-object shapes (CycleTimeReading/ProductionCountReading), because
/// no telemetry ingestion pipeline is persisted yet -- see docs/Phase5/ui-gaps-scoping.md §4 risk
/// note. This is flagged to product as a placeholder pending a real sensor ingestion pipeline.
/// </summary>
public sealed record PredictiveMaintenanceSensorDataPointDto
{
    /// <summary>Sensor channel name (CycleTime, ScrapRate, Downtime).</summary>
    public required string ChannelName { get; init; }

    /// <summary>Reading timestamp (UTC).</summary>
    public required DateTime TimestampUtc { get; init; }

    /// <summary>Sensor reading value.</summary>
    public required double Value { get; init; }

    /// <summary>Unit of measure.</summary>
    public required string Unit { get; init; }
}

/// <summary>
/// A detected anomaly, produced by actually running the real
/// <c>Observability.Telemetry.Services.TelemetryAnomalyDetector</c> domain service against the
/// synthesized readings described on <see cref="PredictiveMaintenanceSensorDataPointDto"/>. The
/// detection algorithm and its thresholds are real; only the underlying readings are placeholders.
/// </summary>
public sealed record PredictiveMaintenanceSensorAnomalyDto
{
    /// <summary>Anomaly timestamp (UTC).</summary>
    public required DateTime TimestampUtc { get; init; }

    /// <summary>Affected channel name.</summary>
    public required string ChannelName { get; init; }

    /// <summary>Anomaly severity (Warning, Critical), derived from the detector's DeviationRatio.</summary>
    public required string Severity { get; init; }

    /// <summary>Reading value at the time of anomaly.</summary>
    public required double Value { get; init; }

    /// <summary>Human-readable explanation, the real TelemetryAnomalyDetector message text.</summary>
    public required string Explanation { get; init; }
}

/// <summary>
/// A monthly predictive-maintenance rollup, grouping real PredictiveMaintenanceAlert records by
/// the calendar month they were predicted in. No PDF/CSV export engine exists yet -- Format is
/// informational only; flagged to product pending a real reporting/export capability.
/// </summary>
public sealed record PredictiveMaintenanceReportDto
{
    /// <summary>Deterministic report identifier derived from the tenant and reporting period.</summary>
    public required Guid ReportId { get; init; }

    /// <summary>Report title.</summary>
    public required string Title { get; init; }

    /// <summary>Reporting period start date.</summary>
    public required DateOnly PeriodStart { get; init; }

    /// <summary>Reporting period end date.</summary>
    public required DateOnly PeriodEnd { get; init; }

    /// <summary>UTC timestamp this rollup was computed.</summary>
    public required DateTime GeneratedAtUtc { get; init; }

    /// <summary>Report format placeholder -- no export engine exists yet.</summary>
    public required string Format { get; init; }
}

/// <summary>Equipment predictive view header, derived from that equipment's most recent PredictiveMaintenanceAlert.</summary>
public sealed record PredictiveMaintenanceEquipmentViewDto
{
    /// <summary>Equipment identifier.</summary>
    public required Guid EquipmentId { get; init; }

    /// <summary>Human-readable equipment code.</summary>
    public required string EquipmentCode { get; init; }

    /// <summary>Equipment display name (currently the same as EquipmentCode -- no separate equipment master data source is wired in yet).</summary>
    public required string EquipmentName { get; init; }

    /// <summary>Current health status (Healthy, Warning, Critical), derived from the most recent alert's status.</summary>
    public required string HealthStatus { get; init; }

    /// <summary>Current normalized risk score (0-1), the FailureLikelihoodPredictor output for the most recent alert.</summary>
    public required double RiskScore { get; init; }

    /// <summary>Most recent active (non-terminal) alert identifier for this equipment, if any.</summary>
    public Guid? LatestPredictionId { get; init; }
}
