// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side item DTOs for the Predictive Analytics
//   module. Mirror the PlantSafety.Application.Analytics.PredictiveAnalytics
//   and PlantSafety.Application.Analytics.Predictive.* read-model item DTOs
//   field-for-field (PredictiveAnalyticsItemDto/ComplianceFailureItemDto/
//   FatigueRiskModelingItemDto/IncidentPredictionItemDto/
//   RiskForecastingItemDto) so PredictiveAnalyticsApiClient can deserialize
//   real backend responses from api/v1/analytics/predictiveanalytics and
//   api/v1/analytics/predictive/{compliancefailure,fatigueriskmodeling,
//   incidentprediction,riskforecasting} directly. Display-only — no domain
//   logic (per Volume A1 UI001, predictive scoring is never computed
//   client-side).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;

namespace PlantSafety.UI.Modules.PredictiveAnalytics.Models;

/// <summary>A single predictive analytics model output (mirrors <c>PredictiveAnalyticsItemDto</c>).</summary>
public sealed class PredictiveModelOutputDto
{
    /// <summary>Gets the prediction identifier.</summary>
    public Guid PredictionId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the name of the predictive model that produced this result.</summary>
    public string ModelName { get; init; } = string.Empty;

    /// <summary>Gets the predicted risk/likelihood score (0.0-1.0).</summary>
    public decimal Score { get; init; }

    /// <summary>Gets the current status (e.g., Generated/Reviewed/Actioned).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when the prediction was generated.</summary>
    public DateTime GeneratedAtUtc { get; init; }

    /// <summary>Gets the identity of the system/user that generated the prediction.</summary>
    public Guid GeneratedBy { get; init; }
}

/// <summary>A single predicted compliance failure risk (mirrors <c>ComplianceFailureItemDto</c>).</summary>
public sealed class ComplianceFailureRiskDto
{
    /// <summary>Gets the prediction identifier.</summary>
    public Guid PredictionId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the compliance area at risk (e.g., Permits, Training, Inspections).</summary>
    public string ComplianceArea { get; init; } = string.Empty;

    /// <summary>Gets the predicted probability of a compliance failure, expressed as a 0-1 value.</summary>
    public decimal FailureProbability { get; init; }

    /// <summary>Gets the risk classification (e.g., Low, Medium, High, Critical).</summary>
    public string RiskLevel { get; init; } = string.Empty;

    /// <summary>Gets the current status (e.g., Active, Mitigated, Dismissed).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when the prediction was generated.</summary>
    public DateTime PredictedAtUtc { get; init; }
}

/// <summary>A single fatigue risk assessment (mirrors <c>FatigueRiskModelingItemDto</c>).</summary>
public sealed class FatigueRiskAssessmentDto
{
    /// <summary>Gets the risk assessment identifier.</summary>
    public Guid RiskAssessmentId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the identifier of the assessed employee.</summary>
    public Guid EmployeeId { get; init; }

    /// <summary>Gets the computed fatigue risk score.</summary>
    public decimal FatigueRiskScore { get; init; }

    /// <summary>Gets the risk classification (e.g., Low, Medium, High, Critical).</summary>
    public string RiskLevel { get; init; } = string.Empty;

    /// <summary>Gets the current status (e.g., Active, Reviewed, Closed).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when the assessment was performed.</summary>
    public DateTime AssessedAtUtc { get; init; }
}

/// <summary>A single predicted incident risk (mirrors <c>IncidentPredictionItemDto</c>).</summary>
public sealed class IncidentPredictionDto
{
    /// <summary>Gets the prediction identifier.</summary>
    public Guid PredictionId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the incident category being predicted (e.g., Slip/Trip/Fall, Equipment, Chemical Exposure).</summary>
    public string IncidentCategory { get; init; } = string.Empty;

    /// <summary>Gets the predicted probability of an incident occurring, expressed as a 0-1 value.</summary>
    public decimal PredictedProbability { get; init; }

    /// <summary>Gets the risk classification (e.g., Low, Medium, High, Critical).</summary>
    public string RiskLevel { get; init; } = string.Empty;

    /// <summary>Gets the current status (e.g., Active, Mitigated, Dismissed).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when the prediction was generated.</summary>
    public DateTime PredictedAtUtc { get; init; }
}

/// <summary>A single forward-looking risk forecast data point (mirrors <c>RiskForecastingItemDto</c>).</summary>
public sealed class RiskForecastPointDto
{
    /// <summary>Gets the forecast identifier.</summary>
    public Guid ForecastId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the risk category being forecast (e.g., Overall Site Risk, Chemical, Ergonomic).</summary>
    public string RiskCategory { get; init; } = string.Empty;

    /// <summary>Gets the forecast risk value for the horizon.</summary>
    public decimal ForecastValue { get; init; }

    /// <summary>Gets the number of days into the future this forecast covers.</summary>
    public int ForecastHorizonDays { get; init; }

    /// <summary>Gets the current status (e.g., Active, Superseded).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when the forecast was generated.</summary>
    public DateTime ForecastedAtUtc { get; init; }
}
