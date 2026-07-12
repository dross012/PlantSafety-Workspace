// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single training class session.
// SchemaVersion: 1

namespace PlantSafety.Application.Operational.Training.Classes.Models;

/// <summary>A single scheduled or completed training class session.</summary>
public sealed record ClassItemDto
{
    /// <summary>Class session identifier.</summary>
    public required Guid ClassId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Class title/subject.</summary>
    public required string Title { get; init; }

    /// <summary>Identity of the assigned instructor.</summary>
    public required Guid InstructorId { get; init; }

    /// <summary>Current status (e.g., Scheduled/InProgress/Completed/Cancelled).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the class is/was scheduled.</summary>
    public required DateTime ScheduledAtUtc { get; init; }
}
