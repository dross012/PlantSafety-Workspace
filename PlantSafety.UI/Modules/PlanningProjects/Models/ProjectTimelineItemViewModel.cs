// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Display-ready view model for a single Gantt-style bar
//   rendered by ProjectTimeline.razor. Shared across Planning Projects
//   (all-projects overview) and Project Scheduling (single-project detail).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;

namespace PlantSafety.UI.Modules.PlanningProjects.Models;

/// <summary>Display-ready timeline bar entry for the Gantt-style ProjectTimeline component.</summary>
public sealed record ProjectTimelineItemViewModel
{
    /// <summary>Gets the project identifier this bar represents.</summary>
    public required Guid ProjectId { get; init; }

    /// <summary>Gets the bar label (project name).</summary>
    public required string Label { get; init; }

    /// <summary>Gets the bar's start date (UTC). Falls back to the snapshot generation time when unknown.</summary>
    public required DateTimeOffset StartUtc { get; init; }

    /// <summary>Gets the bar's end date (UTC) — target or planned completion.</summary>
    public required DateTimeOffset EndUtc { get; init; }

    /// <summary>Gets a secondary end marker (actual completion), or null when not yet completed.</summary>
    public DateTimeOffset? ActualEndUtc { get; init; }

    /// <summary>Gets a value indicating whether <see cref="EndUtc"/> has passed without closure.</summary>
    public required bool IsOverdue { get; init; }

    /// <summary>Gets the raw lifecycle status code, used for bar coloring via SeverityBadgeComponent-equivalent mapping.</summary>
    public required string Status { get; init; }
}
