// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single training course.
// SchemaVersion: 1

namespace PlantSafety.Application.Operational.Training.Courses.Models;

/// <summary>A single training course catalog entry.</summary>
public sealed record CourseItemDto
{
    /// <summary>Course identifier.</summary>
    public required Guid CourseId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Course title.</summary>
    public required string Title { get; init; }

    /// <summary>Course category (e.g., Safety, Compliance, Technical).</summary>
    public required string Category { get; init; }

    /// <summary>Current status (e.g., Draft/Published/Retired).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the course was published.</summary>
    public required DateTime PublishedAtUtc { get; init; }

    /// <summary>Identity of the user who published the course.</summary>
    public required Guid PublishedBy { get; init; }
}
