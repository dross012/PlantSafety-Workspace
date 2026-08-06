// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-05
// FILE DESCRIPTION: Client-side request/response DTOs for the
//   PlanningProjects.ProjectAggregate Create Project and Add Milestone
//   commands. Mirrors PlantSafety.Api.PlanningProjects.Contracts.Requests
//   .ProjectCommandRequests (CreateProjectRequest, AddProjectMilestoneRequest)
//   and PlantSafety.Application.PlanningProjects.ProjectAggregate.Commands
//   .ProjectStatusResult. A Project's TargetCompletionUtc IS the "scheduling"
//   input — ProjectScheduling/ProjectBudgeting are read-only projections over
//   this same data, so no separate scheduling-create endpoint exists.
//   Backed by POST v1/planning-projects/projects[/{projectId}/milestones]
//   (ProjectV1Controller).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.ComponentModel.DataAnnotations;

namespace PlantSafety.UI.Modules.PlanningProjects.Models;

/// <summary>
/// Request body for creating a new Project. Mirrors
/// PlantSafety.Api.PlanningProjects.Contracts.Requests.CreateProjectRequest,
/// with DataAnnotations mirroring CreateProjectCommandValidator's rules
/// (Name required, max length 200). Backed by POST v1/planning-projects/projects.
/// </summary>
public sealed class CreateProjectRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; set; }

    /// <summary>Project name.</summary>
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(200, ErrorMessage = "Name must be 200 characters or fewer.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>UTC timestamp the project is targeted to complete. This is the project's schedule input.</summary>
    [Required(ErrorMessage = "Target completion date is required.")]
    public DateTime TargetCompletionUtc { get; set; } = DateTime.UtcNow.AddMonths(3);
}

/// <summary>
/// Request body for adding a milestone to a Project. Mirrors
/// PlantSafety.Api.PlanningProjects.Contracts.Requests.AddProjectMilestoneRequest.
/// Backed by POST v1/planning-projects/projects/{projectId}/milestones.
/// </summary>
public sealed class AddProjectMilestoneRequest
{
    /// <summary>Milestone name.</summary>
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(200, ErrorMessage = "Name must be 200 characters or fewer.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Target completion date (UTC).</summary>
    [Required(ErrorMessage = "Target date is required.")]
    public DateTime TargetDateUtc { get; set; } = DateTime.UtcNow.AddMonths(1);

    /// <summary>Optimistic-concurrency token from the last read of this Project record. Null skips the check.</summary>
    public string? ETag { get; set; }
}

/// <summary>
/// Result payload shared by Project lifecycle commands (Create, AddMilestone, etc). Mirrors
/// PlantSafety.Application.PlanningProjects.ProjectAggregate.Commands.ProjectStatusResult.
/// </summary>
public sealed class ProjectStatusResult
{
    /// <summary>Gets the identity of the affected Project aggregate.</summary>
    public Guid ProjectId { get; init; }

    /// <summary>Gets the lifecycle status after the transition.</summary>
    public string Status { get; init; } = string.Empty;
}
