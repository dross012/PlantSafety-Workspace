// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-03
// FILE DESCRIPTION: Client-side DTOs for the Admin Portal's Global Safety Dashboard (Phase 8 §5
//   Multi-Site Safety Intelligence Network). Mirrors PlantSafety.Application.GlobalSafetyInsights.*
//   read models and the new GlobalSafetyDashboardV1Controller response contracts.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.Admin.Models;

/// <summary>Read-model projection of a single computed GlobalSafetyInsights snapshot (mirrors <c>GlobalSafetyInsightsItemDto</c>).</summary>
public sealed class GlobalSafetyInsightsItemDto
{
    /// <summary>Gets the snapshot identifier.</summary>
    public Guid GlobalSafetyInsightsId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the region code this snapshot was computed for.</summary>
    public string RegionCode { get; init; } = string.Empty;

    /// <summary>Gets the sites covered by this snapshot.</summary>
    public IReadOnlyList<Guid> SiteIds { get; init; } = [];

    /// <summary>Gets the inclusive UTC start of the analysis window.</summary>
    public DateTime WindowStartUtc { get; init; }

    /// <summary>Gets the inclusive UTC end of the analysis window.</summary>
    public DateTime WindowEndUtc { get; init; }

    /// <summary>Gets the Total Recordable Incident Rate.</summary>
    public decimal Trir { get; init; }

    /// <summary>Gets the Lost Time Incident Rate.</summary>
    public decimal Ltir { get; init; }

    /// <summary>Gets the mean worker exposure-level score across the window.</summary>
    public decimal ExposureRate { get; init; }

    /// <summary>Gets the incidents per site per day across the window.</summary>
    public decimal IncidentDensity { get; init; }

    /// <summary>Gets the Pearson correlation between degraded machine-state observations and incidents.</summary>
    public double MachineStateIncidentCorrelation { get; init; }

    /// <summary>Gets the number of anomalies detected in this snapshot.</summary>
    public int AnomalyCount { get; init; }

    /// <summary>Gets the number of structured recommendations surfaced with this snapshot.</summary>
    public int RecommendationCount { get; init; }

    /// <summary>Gets the UTC instant this snapshot was computed.</summary>
    public DateTime ComputedAtUtc { get; init; }
}

/// <summary>Result envelope for the dashboard's KPI summary endpoint (mirrors <c>GlobalSafetyInsightsQueryResult</c>).</summary>
public sealed class GlobalSafetyInsightsQueryResultDto
{
    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the UTC instant this result was retrieved.</summary>
    public DateTime RetrievedAtUtc { get; init; }

    /// <summary>Gets the most recently computed snapshot, or null when none exists.</summary>
    public GlobalSafetyInsightsItemDto? Latest { get; init; }

    /// <summary>Gets every snapshot within the queried history range, most recent first.</summary>
    public IReadOnlyList<GlobalSafetyInsightsItemDto> History { get; init; } = [];
}

/// <summary>A single site/region/machine-family risk score (mirrors <c>GlobalSafetyRiskScoreDetailDto</c>).</summary>
public sealed class GlobalSafetyRiskScoreDetailDto
{
    /// <summary>Gets which dimension this score was aggregated over ("Site", "Region", "MachineFamily").</summary>
    public string ScopeType { get; init; } = string.Empty;

    /// <summary>Gets the identity of the scope (SiteId, Region code, or machine family name).</summary>
    public string ScopeKey { get; init; } = string.Empty;

    /// <summary>Gets the severity rating (1-5) of the aggregated risk score.</summary>
    public int Severity { get; init; }

    /// <summary>Gets the likelihood rating (1-5) of the aggregated risk score.</summary>
    public int Likelihood { get; init; }

    /// <summary>Gets the computed Severity x Likelihood score.</summary>
    public int Score { get; init; }

    /// <summary>Gets the qualitative risk banding ("Low", "Medium", "High", "Critical").</summary>
    public string RiskLevel { get; init; } = string.Empty;

    /// <summary>Gets the number of underlying Risk records this score was averaged over.</summary>
    public int SourceRiskCount { get; init; }
}

/// <summary>A single detected cross-site/systemic anomaly (mirrors <c>GlobalSafetyAnomalyDetailDto</c>).</summary>
public sealed class GlobalSafetyAnomalyDetailDto
{
    /// <summary>Gets the category of the anomaly.</summary>
    public string Kind { get; init; } = string.Empty;

    /// <summary>Gets the site the anomaly pertains to, or null when it spans a region/machine family.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the machine family the anomaly pertains to, when applicable.</summary>
    public string? MachineFamily { get; init; }

    /// <summary>Gets the statistical magnitude of the deviation.</summary>
    public double DeviationMagnitude { get; init; }

    /// <summary>Gets the human-readable explanation of the anomaly.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Gets the UTC instant the anomaly was detected.</summary>
    public DateTime DetectedAtUtc { get; init; }
}

/// <summary>A single structured safety recommendation (mirrors <c>GlobalSafetyRecommendationDetailDto</c>).</summary>
public sealed class GlobalSafetyRecommendationDetailDto
{
    /// <summary>Gets the urgency of this recommendation.</summary>
    public string Priority { get; init; } = string.Empty;

    /// <summary>Gets the short machine-readable category.</summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>Gets the human-readable recommendation description.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Gets the site this recommendation targets, or null when it targets the whole region/network.</summary>
    public Guid? TargetSiteId { get; init; }

    /// <summary>Gets the kind of anomaly this recommendation was derived from, if any.</summary>
    public string? RelatedAnomalyKind { get; init; }
}

/// <summary>Full detail projection of the most recently computed GlobalSafetyInsights snapshot (mirrors <c>GlobalSafetyInsightsDetailDto</c>).</summary>
public sealed class GlobalSafetyInsightsDetailDto
{
    /// <summary>Gets the snapshot identifier.</summary>
    public Guid GlobalSafetyInsightsId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the region code this snapshot was computed for.</summary>
    public string RegionCode { get; init; } = string.Empty;

    /// <summary>Gets the sites covered by this snapshot.</summary>
    public IReadOnlyList<Guid> SiteIds { get; init; } = [];

    /// <summary>Gets the inclusive UTC start of the analysis window.</summary>
    public DateTime WindowStartUtc { get; init; }

    /// <summary>Gets the inclusive UTC end of the analysis window.</summary>
    public DateTime WindowEndUtc { get; init; }

    /// <summary>Gets the Total Recordable Incident Rate.</summary>
    public decimal Trir { get; init; }

    /// <summary>Gets the Lost Time Incident Rate.</summary>
    public decimal Ltir { get; init; }

    /// <summary>Gets the mean worker exposure-level score across the window.</summary>
    public decimal ExposureRate { get; init; }

    /// <summary>Gets the incidents per site per day across the window.</summary>
    public decimal IncidentDensity { get; init; }

    /// <summary>Gets the Pearson correlation between degraded machine-state observations and incidents.</summary>
    public double MachineStateIncidentCorrelation { get; init; }

    /// <summary>Gets every per-site/region/machine-family risk score in this snapshot.</summary>
    public IReadOnlyList<GlobalSafetyRiskScoreDetailDto> RiskScores { get; init; } = [];

    /// <summary>Gets every anomaly detected in this snapshot.</summary>
    public IReadOnlyList<GlobalSafetyAnomalyDetailDto> Anomalies { get; init; } = [];

    /// <summary>Gets every structured recommendation surfaced with this snapshot.</summary>
    public IReadOnlyList<GlobalSafetyRecommendationDetailDto> Recommendations { get; init; } = [];

    /// <summary>Gets the UTC instant this snapshot was computed.</summary>
    public DateTime ComputedAtUtc { get; init; }
}

/// <summary>A single cross-site coordinated-action audit ledger entry (mirrors <c>CrossSiteActionLogEntryDto</c>).</summary>
public sealed class CrossSiteActionLogEntryDto
{
    /// <summary>Gets the audit ledger entry identifier.</summary>
    public Guid CrossSiteActionLogId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets every site affected by this coordinated action.</summary>
    public IReadOnlyList<Guid> AffectedSiteIds { get; init; } = [];

    /// <summary>Gets the discriminator for the kind of coordinated action.</summary>
    public string ActionType { get; init; } = string.Empty;

    /// <summary>Gets the human-readable description of the action.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Gets the identity of the user who initiated the action, if any.</summary>
    public Guid? InitiatedByUserId { get; init; }

    /// <summary>Gets the UTC instant the action was recorded.</summary>
    public DateTime RecordedAtUtc { get; init; }

    /// <summary>Gets the region the action was resolved/routed in, if known.</summary>
    public string? RegionCode { get; init; }
}
