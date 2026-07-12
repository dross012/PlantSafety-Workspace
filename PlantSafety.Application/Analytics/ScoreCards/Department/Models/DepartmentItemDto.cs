// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single department scorecard entry.
// SchemaVersion: 1

namespace PlantSafety.Application.Analytics.ScoreCards.Department.Models;

/// <summary>A single safety scorecard entry for a department.</summary>
public sealed record DepartmentItemDto
{
    /// <summary>Scorecard entry identifier.</summary>
    public required Guid ScoreCardId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Name of the department scored.</summary>
    public required string DepartmentName { get; init; }

    /// <summary>Score achieved by the department.</summary>
    public required decimal ScoreValue { get; init; }

    /// <summary>Percentile rank of the department relative to peers, if computed.</summary>
    public decimal? RankPercentile { get; init; }

    /// <summary>Current status of the scorecard entry (e.g., Provisional, Final).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the score was calculated.</summary>
    public required DateTime ScoredAtUtc { get; init; }
}
