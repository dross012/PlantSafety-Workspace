// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side item DTOs for the Scorecards module. Mirror
//   the PlantSafety.Application.Analytics.ScoreCards.* read-model item DTOs
//   field-for-field (ComplianceItemDto/DepartmentItemDto/SupervisorItemDto/
//   TrainingItemDto) so ScorecardsApiClient can deserialize real backend
//   responses from api/v1/analytics/scorecards/{compliance,department,
//   supervisor,training} directly (Volume A1 §Page 3 — Scorecard: Score
//   Tiles + Detailed Metrics Table). Display-only — no domain logic.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;

namespace PlantSafety.UI.Modules.Scorecards.Models;

/// <summary>A single compliance scorecard entry (mirrors <c>ComplianceItemDto</c>).</summary>
public sealed class ComplianceScorecardDto
{
    /// <summary>Gets the scorecard entry identifier.</summary>
    public Guid ScoreCardId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the compliance area scored (e.g., Permits, Training, Inspections).</summary>
    public string ComplianceArea { get; init; } = string.Empty;

    /// <summary>Gets the score achieved for this compliance area.</summary>
    public decimal ScoreValue { get; init; }

    /// <summary>Gets the maximum attainable score for this compliance area.</summary>
    public decimal MaxScore { get; init; }

    /// <summary>Gets the current status (e.g., Provisional, Final).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when the score was calculated.</summary>
    public DateTime ScoredAtUtc { get; init; }
}

/// <summary>A single department safety scorecard entry (mirrors <c>DepartmentItemDto</c>).</summary>
public sealed class DepartmentScorecardDto
{
    /// <summary>Gets the scorecard entry identifier.</summary>
    public Guid ScoreCardId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the department name scored.</summary>
    public string DepartmentName { get; init; } = string.Empty;

    /// <summary>Gets the score achieved by the department.</summary>
    public decimal ScoreValue { get; init; }

    /// <summary>Gets the percentile rank relative to peers, if computed.</summary>
    public decimal? RankPercentile { get; init; }

    /// <summary>Gets the current status (e.g., Provisional, Final).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when the score was calculated.</summary>
    public DateTime ScoredAtUtc { get; init; }
}

/// <summary>A single supervisor team scorecard entry (mirrors <c>SupervisorItemDto</c>).</summary>
public sealed class SupervisorScorecardDto
{
    /// <summary>Gets the scorecard entry identifier.</summary>
    public Guid ScoreCardId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the identifier of the scored supervisor.</summary>
    public Guid SupervisorId { get; init; }

    /// <summary>Gets the score achieved by the supervisor's team.</summary>
    public decimal ScoreValue { get; init; }

    /// <summary>Gets the number of direct reports included in the score.</summary>
    public int TeamSize { get; init; }

    /// <summary>Gets the current status (e.g., Provisional, Final).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when the score was calculated.</summary>
    public DateTime ScoredAtUtc { get; init; }
}

/// <summary>A single training completion scorecard entry (mirrors <c>TrainingItemDto</c>).</summary>
public sealed class TrainingScorecardDto
{
    /// <summary>Gets the scorecard entry identifier.</summary>
    public Guid ScoreCardId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the identifier of the scored employee.</summary>
    public Guid EmployeeId { get; init; }

    /// <summary>Gets the training completion rate, expressed as a 0-1 value.</summary>
    public decimal CompletionRate { get; init; }

    /// <summary>Gets the number of required courses completed.</summary>
    public int CoursesCompleted { get; init; }

    /// <summary>Gets the current status (e.g., Provisional, Final).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when the score was calculated.</summary>
    public DateTime ScoredAtUtc { get; init; }
}
