// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PlanningProjects.ProjectAggregate
//  Layer:          Application
//  File:           ProjectDetailDto.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Flat read-model item DTO for a single Project record, including every
//                  aggregate property plus an ETag. Backs GetProjectByIdQuery. Distinct from the
//                  pre-existing, simpler PlanningProjectItemDto used by the list-range query
//                  (GetPlanningProjectsQuery), which is left untouched. Mirrors
//                  PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Models
//                  .RootCauseAnalysisDetailDto's shape.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.PlanningProjects.ProjectAggregate.Models;

namespace PlantSafety.Application.PlanningProjects.ProjectAggregate.Models;

/// <summary>A single Project read-model record, including the full lifecycle state.</summary>
public sealed record ProjectDetailDto
{
    /// <summary>Record identifier.</summary>
    public required Guid ProjectId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Project name.</summary>
    public required string Name { get; init; }

    /// <summary>Current lifecycle status (Planning/Active/OnHold/Completed/Cancelled/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the project is targeted to complete.</summary>
    public required DateTime TargetCompletionUtc { get; init; }

    /// <summary>Identity of the project owner.</summary>
    public required Guid OwnerId { get; init; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }

    /// <summary>Milestones belonging to this project.</summary>
    public IReadOnlyList<ProjectMilestoneDto> Milestones { get; init; } = [];

    /// <summary>
    /// Optimistic-concurrency token (native RowVersion projected as a hex string). Callers must
    /// echo this value back on a subsequent mutating command's ETag field; a stale/mismatched
    /// value results in a concurrency-conflict error.
    /// </summary>
    public string? ETag { get; init; }
}
