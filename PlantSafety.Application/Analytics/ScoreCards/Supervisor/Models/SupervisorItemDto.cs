// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single supervisor scorecard entry.
// SchemaVersion: 1

namespace PlantSafety.Application.Analytics.ScoreCards.Supervisor.Models;

/// <summary>A single safety scorecard entry for a supervisor and their team.</summary>
public sealed record SupervisorItemDto
{
    /// <summary>Scorecard entry identifier.</summary>
    public required Guid ScoreCardId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identifier of the scored supervisor.</summary>
    public required Guid SupervisorId { get; init; }

    /// <summary>Score achieved by the supervisor's team.</summary>
    public required decimal ScoreValue { get; init; }

    /// <summary>Number of direct reports included in the score.</summary>
    public required int TeamSize { get; init; }

    /// <summary>Current status of the scorecard entry (e.g., Provisional, Final).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the score was calculated.</summary>
    public required DateTime ScoredAtUtc { get; init; }
}
