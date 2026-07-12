// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single compliance scorecard entry.
// SchemaVersion: 1

namespace PlantSafety.Application.Analytics.ScoreCards.Compliance.Models;

/// <summary>A single compliance scorecard entry for a given compliance area.</summary>
public sealed record ComplianceItemDto
{
    /// <summary>Scorecard entry identifier.</summary>
    public required Guid ScoreCardId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Compliance area scored (e.g., Permits, Training, Inspections).</summary>
    public required string ComplianceArea { get; init; }

    /// <summary>Score achieved for this compliance area.</summary>
    public required decimal ScoreValue { get; init; }

    /// <summary>Maximum attainable score for this compliance area.</summary>
    public required decimal MaxScore { get; init; }

    /// <summary>Current status of the scorecard entry (e.g., Provisional, Final).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the score was calculated.</summary>
    public required DateTime ScoredAtUtc { get; init; }
}
