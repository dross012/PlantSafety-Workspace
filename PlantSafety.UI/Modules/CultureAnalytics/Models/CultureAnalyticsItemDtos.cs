// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side item DTOs for the Culture Analytics module.
//   Mirror the PlantSafety.Application.Analytics.Culture.* read-model item
//   DTOs field-for-field (BehaviorItemDto/CultureIndexItemDto/
//   EngagementScoringItemDto/TrendModelingItemDto) so CultureAnalyticsApiClient
//   can deserialize real backend responses from api/v1/analytics/culture/
//   {behavior,cultureindex,engagementscoring,trendmodeling} directly.
//   Display-only — no domain logic.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;

namespace PlantSafety.UI.Modules.CultureAnalytics.Models;

/// <summary>A single safety behavior observation (mirrors <c>BehaviorItemDto</c>).</summary>
public sealed class BehaviorObservationDto
{
    /// <summary>Gets the observation identifier.</summary>
    public Guid ObservationId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the identifier of the employee observed.</summary>
    public Guid EmployeeId { get; init; }

    /// <summary>Gets the behavior category (e.g., PPE Compliance, Housekeeping, Ergonomics).</summary>
    public string BehaviorCategory { get; init; } = string.Empty;

    /// <summary>Gets the observation classification (e.g., Safe, AtRisk).</summary>
    public string ObservationType { get; init; } = string.Empty;

    /// <summary>Gets the current status (e.g., Open, Reviewed, Closed).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when the observation was recorded.</summary>
    public DateTime ObservedAtUtc { get; init; }

    /// <summary>Gets the identity of the observer who recorded the entry.</summary>
    public Guid ObservedBy { get; init; }
}

/// <summary>A single composite safety culture index snapshot (mirrors <c>CultureIndexItemDto</c>).</summary>
public sealed class CultureIndexSnapshotDto
{
    /// <summary>Gets the index snapshot identifier.</summary>
    public Guid IndexSnapshotId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the culture dimension measured (e.g., Trust, Communication, Accountability).</summary>
    public string CultureDimension { get; init; } = string.Empty;

    /// <summary>Gets the composite index score for this dimension.</summary>
    public decimal IndexScore { get; init; }

    /// <summary>Gets the trend direction versus the prior period (e.g., Improving, Stable, Declining).</summary>
    public string TrendDirection { get; init; } = string.Empty;

    /// <summary>Gets the current status (e.g., Draft, Published).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when the index was calculated.</summary>
    public DateTime CalculatedAtUtc { get; init; }
}

/// <summary>A single employee engagement score (mirrors <c>EngagementScoringItemDto</c>).</summary>
public sealed class EngagementScoreDto
{
    /// <summary>Gets the score identifier.</summary>
    public Guid ScoreId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the identifier of the scored employee.</summary>
    public Guid EmployeeId { get; init; }

    /// <summary>Gets the engagement score value.</summary>
    public decimal EngagementScore { get; init; }

    /// <summary>Gets the category of the scoring source (e.g., Survey, Participation, Recognition).</summary>
    public string ScoreCategory { get; init; } = string.Empty;

    /// <summary>Gets the current status (e.g., Provisional, Final).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when the score was calculated.</summary>
    public DateTime ScoredAtUtc { get; init; }
}

/// <summary>A single predicted culture trend data point (mirrors <c>TrendModelingItemDto</c>).</summary>
public sealed class CultureTrendModelDto
{
    /// <summary>Gets the trend model output identifier.</summary>
    public Guid TrendModelId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the name of the culture metric being modeled.</summary>
    public string MetricName { get; init; } = string.Empty;

    /// <summary>Gets the predicted value of the metric for the forecast period.</summary>
    public decimal PredictedValue { get; init; }

    /// <summary>Gets the model confidence for the prediction, expressed as a 0-1 value.</summary>
    public decimal ModelConfidence { get; init; }

    /// <summary>Gets the current status (e.g., Draft, Validated).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when the prediction was generated.</summary>
    public DateTime GeneratedAtUtc { get; init; }
}
