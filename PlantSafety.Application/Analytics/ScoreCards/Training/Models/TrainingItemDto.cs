// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single training scorecard entry.
// SchemaVersion: 1

namespace PlantSafety.Application.Analytics.ScoreCards.Training.Models;

/// <summary>A single training completion scorecard entry for an employee.</summary>
public sealed record TrainingItemDto
{
    /// <summary>Scorecard entry identifier.</summary>
    public required Guid ScoreCardId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identifier of the scored employee.</summary>
    public required Guid EmployeeId { get; init; }

    /// <summary>Training completion rate, expressed as a 0-1 value.</summary>
    public required decimal CompletionRate { get; init; }

    /// <summary>Number of required courses completed.</summary>
    public required int CoursesCompleted { get; init; }

    /// <summary>Current status of the scorecard entry (e.g., Provisional, Final).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the score was calculated.</summary>
    public required DateTime ScoredAtUtc { get; init; }
}
