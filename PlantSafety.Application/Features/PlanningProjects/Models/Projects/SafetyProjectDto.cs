// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-side DTO for a Safety Project record.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.PlanningProjects.Models.Projects;

/// <summary>
/// Read-side projection of a SafetyProject aggregate.
/// Safety projects are planned improvement initiatives driven by incidents, audits, or compliance requirements.
/// Lifecycle: Draft → Planned → InProgress → Blocked → Completed → Archived.
/// </summary>
public sealed record SafetyProjectDto
{
    /// <summary>Unique identifier (UUIDv7).</summary>
    public required Guid ProjectId { get; init; }

    /// <summary>Tenant owning this project.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Primary site for this project.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Project name. Max 200 characters.</summary>
    public required string Name { get; init; }

    /// <summary>Detailed description of scope and objectives. Max 4000 characters.</summary>
    public required string Description { get; init; }

    /// <summary>Project lifecycle status.</summary>
    public required string Status { get; init; }

    /// <summary>Priority (Low, Medium, High, Critical).</summary>
    public required string Priority { get; init; }

    /// <summary>Project owner/sponsor.</summary>
    public required Guid ProjectOwnerId { get; init; }

    /// <summary>Project manager.</summary>
    public Guid? ProjectManagerId { get; init; }

    /// <summary>Planned start date (UTC).</summary>
    public required DateTimeOffset PlannedStartUtc { get; init; }

    /// <summary>Planned completion date (UTC).</summary>
    public required DateTimeOffset PlannedEndUtc { get; init; }

    /// <summary>Actual start date (UTC), if started.</summary>
    public DateTimeOffset? ActualStartUtc { get; init; }

    /// <summary>Actual completion date (UTC), if completed.</summary>
    public DateTimeOffset? ActualEndUtc { get; init; }

    /// <summary>Total planned budget for this project.</summary>
    public decimal? PlannedBudget { get; init; }

    /// <summary>Number of open milestones.</summary>
    public required int OpenMilestoneCount { get; init; }

    /// <summary>UTC timestamp of last modification.</summary>
    public required DateTimeOffset LastModifiedAtUtc { get; init; }

    /// <summary>Row version for concurrency.</summary>
    public required string ETag { get; init; }
}
