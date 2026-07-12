// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single seminar session.
// SchemaVersion: 1

namespace PlantSafety.Application.Operational.Training.Seminars.Models;

/// <summary>A single scheduled or completed seminar session.</summary>
public sealed record SeminarItemDto
{
    /// <summary>Seminar identifier.</summary>
    public required Guid SeminarId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Seminar title/topic.</summary>
    public required string Title { get; init; }

    /// <summary>Identity of the assigned presenter.</summary>
    public required Guid PresenterId { get; init; }

    /// <summary>Current status (e.g., Scheduled/InProgress/Completed/Cancelled).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the seminar is/was scheduled.</summary>
    public required DateTime ScheduledAtUtc { get; init; }
}
