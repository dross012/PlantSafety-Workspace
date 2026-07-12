// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single employee engagement score.
// SchemaVersion: 1

namespace PlantSafety.Application.Analytics.Culture.EngagementScoring.Models;

/// <summary>A single engagement score recorded for an employee.</summary>
public sealed record EngagementScoringItemDto
{
    /// <summary>Score identifier.</summary>
    public required Guid ScoreId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identifier of the scored employee.</summary>
    public required Guid EmployeeId { get; init; }

    /// <summary>Engagement score value.</summary>
    public required decimal EngagementScore { get; init; }

    /// <summary>Category of the scoring source (e.g., Survey, Participation, Recognition).</summary>
    public required string ScoreCategory { get; init; }

    /// <summary>Current status of the score record (e.g., Provisional, Final).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the score was calculated.</summary>
    public required DateTime ScoredAtUtc { get; init; }
}
