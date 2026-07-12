// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single planning project.
// SchemaVersion: 1

namespace PlantSafety.Application.PlanningProjects.PlanningProjects.Models;

/// <summary>A single strategic planning project.</summary>
public sealed record PlanningProjectItemDto
{
    /// <summary>Project identifier.</summary>
    public required Guid ProjectId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Project name.</summary>
    public required string Name { get; init; }

    /// <summary>Current status (e.g., Planning/InProgress/OnHold/Completed).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the project is targeted to complete.</summary>
    public required DateTime TargetCompletionUtc { get; init; }

    /// <summary>Identity of the project owner.</summary>
    public required Guid OwnerId { get; init; }
}
