// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Application
//  File:           GlobalSafetyInsightsDetailDto.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §5 Global Safety Dashboard)
//  SchemaVersion:  2026.1
//  Description:    Read-model DTOs projecting a computed GlobalSafetyInsightsAggregate's full
//                  detail (per-scope risk scores, anomaly detail, recommendation detail) for the
//                  Admin/Executive Global Safety Dashboard's anomaly-map and region-heatmap
//                  widgets. GetGlobalSafetyInsightsQuery/GlobalSafetyInsightsItemDto intentionally
//                  only projects counts (AnomalyCount/RecommendationCount) for the lighter-weight
//                  history list; this detail DTO composes the real per-item data those widgets need
//                  directly from the Domain repository's hydrated aggregate. Additive-only
//                  evolution.
// ============================================================================
namespace PlantSafety.Application.GlobalSafetyInsights.Models;

/// <summary>A single site/region/machine-family risk score, projected for the dashboard.</summary>
public sealed record GlobalSafetyRiskScoreDetailDto
{
    /// <summary>Which dimension this score was aggregated over ("Site", "Region", "MachineFamily").</summary>
    public required string ScopeType { get; init; }

    /// <summary>Identity of the scope (SiteId, Region code, or machine family name).</summary>
    public required string ScopeKey { get; init; }

    /// <summary>Severity rating (1-5) of the aggregated risk score.</summary>
    public required int Severity { get; init; }

    /// <summary>Likelihood rating (1-5) of the aggregated risk score.</summary>
    public required int Likelihood { get; init; }

    /// <summary>Computed Severity x Likelihood score.</summary>
    public required int Score { get; init; }

    /// <summary>Qualitative risk banding ("Low", "Medium", "High", "Critical").</summary>
    public required string RiskLevel { get; init; }

    /// <summary>Number of underlying Risk records this score was averaged over.</summary>
    public required int SourceRiskCount { get; init; }
}

/// <summary>A single detected cross-site/systemic anomaly, projected for the dashboard.</summary>
public sealed record GlobalSafetyAnomalyDetailDto
{
    /// <summary>Category of the anomaly (e.g. "CrossSiteIncidentSpike").</summary>
    public required string Kind { get; init; }

    /// <summary>Site the anomaly pertains to, or null when it spans a region/machine family.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Machine family the anomaly pertains to, when applicable.</summary>
    public string? MachineFamily { get; init; }

    /// <summary>Statistical magnitude of the deviation (z-score or deviation ratio, per Kind).</summary>
    public required double DeviationMagnitude { get; init; }

    /// <summary>Human-readable explanation of the anomaly.</summary>
    public required string Message { get; init; }

    /// <summary>UTC instant the anomaly was detected.</summary>
    public required DateTime DetectedAtUtc { get; init; }
}

/// <summary>A single structured safety recommendation, projected for the dashboard.</summary>
public sealed record GlobalSafetyRecommendationDetailDto
{
    /// <summary>Urgency of this recommendation ("Low", "Medium", "High", "Critical").</summary>
    public required string Priority { get; init; }

    /// <summary>Short machine-readable category.</summary>
    public required string Category { get; init; }

    /// <summary>Human-readable recommendation description.</summary>
    public required string Description { get; init; }

    /// <summary>Site this recommendation targets, or null when it targets the whole region/network.</summary>
    public Guid? TargetSiteId { get; init; }

    /// <summary>Kind of anomaly this recommendation was derived from, if any.</summary>
    public string? RelatedAnomalyKind { get; init; }
}

/// <summary>Full detail projection of the most recently computed GlobalSafetyInsights snapshot.</summary>
public sealed record GlobalSafetyInsightsDetailDto
{
    /// <summary>Snapshot identifier.</summary>
    public required Guid GlobalSafetyInsightsId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Region code this snapshot was computed for.</summary>
    public required string RegionCode { get; init; }

    /// <summary>Sites covered by this snapshot.</summary>
    public required IReadOnlyList<Guid> SiteIds { get; init; }

    /// <summary>Inclusive UTC start of the analysis window.</summary>
    public required DateTime WindowStartUtc { get; init; }

    /// <summary>Inclusive UTC end of the analysis window.</summary>
    public required DateTime WindowEndUtc { get; init; }

    /// <summary>Total Recordable Incident Rate.</summary>
    public required decimal Trir { get; init; }

    /// <summary>Lost Time Incident Rate.</summary>
    public required decimal Ltir { get; init; }

    /// <summary>Mean worker exposure-level score across the window.</summary>
    public required decimal ExposureRate { get; init; }

    /// <summary>Incidents per site per day across the window.</summary>
    public required decimal IncidentDensity { get; init; }

    /// <summary>Pearson correlation between degraded machine-state observations and incidents.</summary>
    public required double MachineStateIncidentCorrelation { get; init; }

    /// <summary>Every per-site/region/machine-family risk score in this snapshot.</summary>
    public required IReadOnlyList<GlobalSafetyRiskScoreDetailDto> RiskScores { get; init; }

    /// <summary>Every anomaly detected in this snapshot.</summary>
    public required IReadOnlyList<GlobalSafetyAnomalyDetailDto> Anomalies { get; init; }

    /// <summary>Every structured recommendation surfaced with this snapshot.</summary>
    public required IReadOnlyList<GlobalSafetyRecommendationDetailDto> Recommendations { get; init; }

    /// <summary>UTC instant this snapshot was computed.</summary>
    public required DateTime ComputedAtUtc { get; init; }
}
