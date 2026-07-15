// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Display-ready view model for the Planning Project /
//   Project Budgeting / Project Scheduling detail pages. Produced by
//   PlanningProjectsMappingService from SafetyProjectDto.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;

namespace PlantSafety.UI.Modules.PlanningProjects.Models;

/// <summary>Display-ready single-project detail view model.</summary>
public sealed record ProjectDetailViewModel
{
    /// <summary>Gets the project identifier.</summary>
    public required Guid ProjectId { get; init; }

    /// <summary>Gets the project name.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the project description.</summary>
    public required string Description { get; init; }

    /// <summary>Gets the raw lifecycle status code.</summary>
    public required string Status { get; init; }

    /// <summary>Gets the StatusIndicatorComponent operational state derived from <see cref="Status"/>.</summary>
    public required string StatusIndicatorState { get; init; }

    /// <summary>Gets the priority code (Low, Medium, High, Critical) — rendered via SeverityBadgeComponent.</summary>
    public required string Priority { get; init; }

    /// <summary>Gets the project owner identifier.</summary>
    public required Guid ProjectOwnerId { get; init; }

    /// <summary>Gets the project manager identifier, or null when unassigned.</summary>
    public Guid? ProjectManagerId { get; init; }

    /// <summary>Gets the planned start date (UTC).</summary>
    public required DateTimeOffset PlannedStartUtc { get; init; }

    /// <summary>Gets the planned completion date (UTC).</summary>
    public required DateTimeOffset PlannedEndUtc { get; init; }

    /// <summary>Gets the actual start date (UTC), or null when not yet started.</summary>
    public DateTimeOffset? ActualStartUtc { get; init; }

    /// <summary>Gets the actual completion date (UTC), or null when not yet completed.</summary>
    public DateTimeOffset? ActualEndUtc { get; init; }

    /// <summary>Gets the planned budget, or null when not set.</summary>
    public decimal? PlannedBudget { get; init; }

    /// <summary>Gets the count of open (unresolved) milestones.</summary>
    public required int OpenMilestoneCount { get; init; }

    /// <summary>Gets the UTC timestamp of last modification.</summary>
    public required DateTimeOffset LastModifiedAtUtc { get; init; }

    /// <summary>Gets a value indicating whether <see cref="PlannedEndUtc"/> has passed without an actual completion.</summary>
    public required bool IsOverdue { get; init; }
}
