// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTO mirroring the server-side SafetyProjectDto
//   returned by GET /api/v1/features/planning-projects/projects/{projectId}.
//   Read-side projection of a SafetyProject aggregate — planned improvement
//   initiatives driven by incidents, audits, or compliance requirements.
//   Lifecycle: Draft -> Planned -> InProgress -> Blocked -> Completed -> Archived.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;

namespace PlantSafety.UI.Modules.PlanningProjects.Models;

/// <summary>Full Safety Project detail DTO returned by the Projects detail endpoint.</summary>
public sealed class SafetyProjectDto
{
    /// <summary>Gets the unique identifier (UUIDv7).</summary>
    public Guid ProjectId { get; init; }

    /// <summary>Gets the tenant owning this project.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the primary site for this project.</summary>
    public Guid SiteId { get; init; }

    /// <summary>Gets the project name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets the detailed description of scope and objectives.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Gets the project lifecycle status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the priority (Low, Medium, High, Critical).</summary>
    public string Priority { get; init; } = string.Empty;

    /// <summary>Gets the project owner/sponsor identifier.</summary>
    public Guid ProjectOwnerId { get; init; }

    /// <summary>Gets the project manager identifier, if assigned.</summary>
    public Guid? ProjectManagerId { get; init; }

    /// <summary>Gets the planned start date (UTC).</summary>
    public DateTimeOffset PlannedStartUtc { get; init; }

    /// <summary>Gets the planned completion date (UTC).</summary>
    public DateTimeOffset PlannedEndUtc { get; init; }

    /// <summary>Gets the actual start date (UTC), if started.</summary>
    public DateTimeOffset? ActualStartUtc { get; init; }

    /// <summary>Gets the actual completion date (UTC), if completed.</summary>
    public DateTimeOffset? ActualEndUtc { get; init; }

    /// <summary>Gets the total planned budget for this project, if known.</summary>
    public decimal? PlannedBudget { get; init; }

    /// <summary>Gets the number of open milestones.</summary>
    public int OpenMilestoneCount { get; init; }

    /// <summary>Gets the UTC timestamp of last modification.</summary>
    public DateTimeOffset LastModifiedAtUtc { get; init; }

    /// <summary>Gets the row version for concurrency (ETag).</summary>
    public string ETag { get; init; } = string.Empty;
}
