// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTOs for the Predictive Maintenance module
//   (Volume P1) — Dashboard, Equipment Predictive View, Predictions List,
//   Prediction Detail, Sensor Data Explorer, Recommendation Detail, and
//   Predictive Reports pages. Display-only — no model/risk-scoring logic.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.PredictiveMaintenance.Models;

/// <summary>Risk Summary card counts (High/Medium/Low).</summary>
public sealed class RiskSummaryDto
{
    /// <summary>Gets the count of high-risk predictions.</summary>
    public int HighRiskCount { get; init; }

    /// <summary>Gets the count of medium-risk predictions.</summary>
    public int MediumRiskCount { get; init; }

    /// <summary>Gets the count of low-risk predictions.</summary>
    public int LowRiskCount { get; init; }

    /// <summary>Gets the overall trend direction (Increasing, Decreasing, Stable).</summary>
    public string TrendDirection { get; init; } = string.Empty;
}

/// <summary>Equipment Health card percentages (Healthy/Warning/Critical).</summary>
public sealed class EquipmentHealthDto
{
    /// <summary>Gets the percentage of equipment in a healthy state.</summary>
    public double HealthyPercent { get; init; }

    /// <summary>Gets the percentage of equipment in a warning state.</summary>
    public double WarningPercent { get; init; }

    /// <summary>Gets the percentage of equipment in a critical state.</summary>
    public double CriticalPercent { get; init; }
}

/// <summary>Single row in the Active Predictions table.</summary>
public sealed class PredictionRowDto
{
    /// <summary>Gets the prediction identifier (UUIDv7).</summary>
    public Guid PredictionId { get; init; }

    /// <summary>Gets the human-readable prediction code (format P-###).</summary>
    public string PredictionCode { get; init; } = string.Empty;

    /// <summary>Gets the target equipment identifier.</summary>
    public Guid EquipmentId { get; init; }

    /// <summary>Gets the human-readable equipment code (format EQ-###).</summary>
    public string EquipmentCode { get; init; } = string.Empty;

    /// <summary>Gets the risk level (High, Medium, Low).</summary>
    public string RiskLevel { get; init; } = string.Empty;

    /// <summary>Gets the model confidence percentage (0-100).</summary>
    public double ConfidencePercent { get; init; }

    /// <summary>Gets the UTC timestamp this prediction was last updated.</summary>
    public DateTimeOffset UpdatedAtUtc { get; init; }

    /// <summary>Gets a value indicating whether this prediction is still active (not superseded/historical).</summary>
    public bool IsActive { get; init; }
}

/// <summary>Prediction Summary header for the Prediction Detail page.</summary>
public sealed class PredictionSummaryDto
{
    /// <summary>Gets the prediction identifier.</summary>
    public Guid PredictionId { get; init; }

    /// <summary>Gets the human-readable prediction code.</summary>
    public string PredictionCode { get; init; } = string.Empty;

    /// <summary>Gets the target equipment code.</summary>
    public string EquipmentCode { get; init; } = string.Empty;

    /// <summary>Gets the target equipment identifier.</summary>
    public Guid EquipmentId { get; init; }

    /// <summary>Gets the normalized risk score (0-1).</summary>
    public double RiskScore { get; init; }

    /// <summary>Gets the model confidence percentage (0-100).</summary>
    public double ConfidencePercent { get; init; }

    /// <summary>Gets the UTC timestamp this prediction was last updated.</summary>
    public DateTimeOffset UpdatedAtUtc { get; init; }

    /// <summary>Gets the data freshness description (e.g. "Sensor data current as of 2 minutes ago").</summary>
    public string DataFreshness { get; init; } = string.Empty;
}

/// <summary>Single ranked contributing factor for the Model Explanation panel.</summary>
public sealed class FeatureImportanceDto
{
    /// <summary>Gets the feature/variable name (e.g. "Temperature").</summary>
    public string FeatureName { get; init; } = string.Empty;

    /// <summary>Gets the normalized contribution (sum across all factors = 1.0).</summary>
    public double Contribution { get; init; }

    /// <summary>Gets a value indicating whether this factor increases (true) or decreases (false) risk.</summary>
    public bool IsPositiveInfluence { get; init; }
}

/// <summary>Model explanation panel data (feature importance + model metadata).</summary>
public sealed class ModelExplanationDto
{
    /// <summary>Gets the ranked contributing factors.</summary>
    public IReadOnlyList<FeatureImportanceDto> Factors { get; init; } = [];

    /// <summary>Gets the traceable model version identifier.</summary>
    public string ModelVersion { get; init; } = string.Empty;

    /// <summary>Gets a plain-language explanation summary.</summary>
    public string ExplanationSummary { get; init; } = string.Empty;
}

/// <summary>Single point on the risk forecast curve.</summary>
public sealed class ForecastPointDto
{
    /// <summary>Gets the forecast timestamp (UTC).</summary>
    public DateTimeOffset TimestampUtc { get; init; }

    /// <summary>Gets the forecasted risk score (0-1) at this timestamp.</summary>
    public double RiskScore { get; init; }

    /// <summary>Gets the lower confidence interval bound.</summary>
    public double ConfidenceLower { get; init; }

    /// <summary>Gets the upper confidence interval bound.</summary>
    public double ConfidenceUpper { get; init; }
}

/// <summary>Single recommended corrective action.</summary>
public sealed class RecommendationDto
{
    /// <summary>Gets the recommendation identifier.</summary>
    public Guid RecommendationId { get; init; }

    /// <summary>Gets the recommendation title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Gets the priority (Low, Medium, High, Critical).</summary>
    public string Priority { get; init; } = string.Empty;

    /// <summary>Gets the ordered step-by-step guidance.</summary>
    public IReadOnlyList<string> Steps { get; init; } = [];

    /// <summary>Gets the estimated risk reduction if followed (0-1).</summary>
    public double EstimatedRiskReduction { get; init; }
}

/// <summary>Single sensor data channel time-series point.</summary>
public sealed class SensorDataPointDto
{
    /// <summary>Gets the sensor channel name.</summary>
    public string ChannelName { get; init; } = string.Empty;

    /// <summary>Gets the reading timestamp (UTC).</summary>
    public DateTimeOffset TimestampUtc { get; init; }

    /// <summary>Gets the sensor reading value.</summary>
    public double Value { get; init; }

    /// <summary>Gets the unit of measure.</summary>
    public string Unit { get; init; } = string.Empty;
}

/// <summary>A detected anomaly on a sensor channel.</summary>
public sealed class SensorAnomalyDto
{
    /// <summary>Gets the anomaly timestamp (UTC).</summary>
    public DateTimeOffset TimestampUtc { get; init; }

    /// <summary>Gets the affected channel name.</summary>
    public string ChannelName { get; init; } = string.Empty;

    /// <summary>Gets the anomaly severity.</summary>
    public string Severity { get; init; } = string.Empty;

    /// <summary>Gets the reading value at the time of anomaly.</summary>
    public double Value { get; init; }

    /// <summary>Gets a human-readable explanation of the anomaly.</summary>
    public string Explanation { get; init; } = string.Empty;
}

/// <summary>Single exportable predictive report summary.</summary>
public sealed class PredictiveReportDto
{
    /// <summary>Gets the report identifier.</summary>
    public Guid ReportId { get; init; }

    /// <summary>Gets the report title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Gets the reporting period start date.</summary>
    public DateOnly PeriodStart { get; init; }

    /// <summary>Gets the reporting period end date.</summary>
    public DateOnly PeriodEnd { get; init; }

    /// <summary>Gets the UTC timestamp the report was generated.</summary>
    public DateTimeOffset GeneratedAtUtc { get; init; }

    /// <summary>Gets the report format (PDF, CSV, XLSX).</summary>
    public string Format { get; init; } = string.Empty;
}

/// <summary>Equipment predictive view header (Volume P1 §2).</summary>
public sealed class EquipmentPredictiveViewDto
{
    /// <summary>Gets the equipment identifier.</summary>
    public Guid EquipmentId { get; init; }

    /// <summary>Gets the human-readable equipment code.</summary>
    public string EquipmentCode { get; init; } = string.Empty;

    /// <summary>Gets the equipment display name.</summary>
    public string EquipmentName { get; init; } = string.Empty;

    /// <summary>Gets the current health status (Healthy, Warning, Critical).</summary>
    public string HealthStatus { get; init; } = string.Empty;

    /// <summary>Gets the current normalized risk score (0-1).</summary>
    public double RiskScore { get; init; }

    /// <summary>Gets the most recent active prediction identifier for this equipment, if any.</summary>
    public Guid? LatestPredictionId { get; init; }
}
