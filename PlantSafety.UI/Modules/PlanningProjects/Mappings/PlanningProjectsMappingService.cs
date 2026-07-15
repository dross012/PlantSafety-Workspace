// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: UI-layer mapping service that projects wire DTOs
//   (PlanningProjectListItemDto / SafetyProjectDto) into display-ready view
//   models (ProjectCardViewModel / ProjectDetailViewModel /
//   ProjectTimelineItemViewModel). Named and shaped after the platform's
//   standard Mapster DTO -> ViewModel convention (Adapt-style pure static
//   projections, no side effects); implemented as hand-written static
//   methods rather than the Mapster package because PlantSafety.UI.csproj
//   does not currently reference Mapster (no other module in this project
//   does either) — adding a new package dependency was judged out of scope
//   for a UI-only batch. Shared by the Planning Projects, Project Budgeting,
//   and Project Scheduling modules, which all render the same underlying
//   SafetyProject/PlanningProjectItem data through different lenses.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;

using PlantSafety.UI.Modules.PlanningProjects.Models;

namespace PlantSafety.UI.Modules.PlanningProjects.Mappings;

/// <summary>Pure, stateless DTO -&gt; ViewModel projections for the Planning Projects family of modules.</summary>
public static class PlanningProjectsMappingService
{
    /// <summary>Projects a list-item DTO into a display-ready card view model.</summary>
    /// <param name="dto">The wire DTO from the PlanningProjects list endpoint.</param>
    /// <param name="nowUtc">The current UTC instant, used to compute overdue/remaining-day fields.</param>
    public static ProjectCardViewModel ToCardViewModel(this PlanningProjectListItemDto dto, DateTime nowUtc)
    {
        var isOverdue = dto.TargetCompletionUtc < nowUtc && !IsTerminalStatus(dto.Status);

        return new ProjectCardViewModel
        {
            ProjectId = dto.ProjectId,
            Name = dto.Name,
            Status = dto.Status,
            StatusIndicatorState = ToStatusIndicatorState(dto.Status),
            TargetCompletionUtc = dto.TargetCompletionUtc,
            OwnerId = dto.OwnerId,
            DaysRemaining = (int)Math.Floor((dto.TargetCompletionUtc - nowUtc).TotalDays),
            IsOverdue = isOverdue,
        };
    }

    /// <summary>Projects a list of list-item DTOs into card view models, ordered by soonest target completion first.</summary>
    /// <param name="items">The wire DTOs from the PlanningProjects list endpoint.</param>
    /// <param name="nowUtc">The current UTC instant, used to compute overdue/remaining-day fields.</param>
    public static IReadOnlyList<ProjectCardViewModel> ToCardViewModels(
        this IEnumerable<PlanningProjectListItemDto> items,
        DateTime nowUtc)
        => items.Select(i => i.ToCardViewModel(nowUtc))
                .OrderBy(vm => vm.TargetCompletionUtc)
                .ToList();

    /// <summary>Projects a list-item DTO into a Gantt-bar timeline view model.</summary>
    /// <remarks>
    /// The PlanningProjects list endpoint does not return a project start date — only a
    /// target completion date — so the bar's start is anchored at <paramref name="snapshotUtc"/>
    /// (the read-model snapshot generation time) to represent "time remaining" rather than
    /// full project duration. Use <see cref="ToTimelineItemViewModel(SafetyProjectDto)"/> on the
    /// detail page, where true PlannedStartUtc/PlannedEndUtc are available.
    /// </remarks>
    /// <param name="dto">The wire DTO from the PlanningProjects list endpoint.</param>
    /// <param name="snapshotUtc">The read-model snapshot generation time (bar start anchor).</param>
    public static ProjectTimelineItemViewModel ToTimelineItemViewModel(this PlanningProjectListItemDto dto, DateTime snapshotUtc)
        => new()
        {
            ProjectId = dto.ProjectId,
            Label = dto.Name,
            StartUtc = snapshotUtc,
            EndUtc = dto.TargetCompletionUtc,
            ActualEndUtc = null,
            IsOverdue = dto.TargetCompletionUtc < snapshotUtc && !IsTerminalStatus(dto.Status),
            Status = dto.Status,
        };

    /// <summary>Projects a full Safety Project detail DTO into a Gantt-bar timeline view model using its true planned dates.</summary>
    /// <param name="dto">The wire DTO from the Project detail endpoint.</param>
    public static ProjectTimelineItemViewModel ToTimelineItemViewModel(this SafetyProjectDto dto)
        => new()
        {
            ProjectId = dto.ProjectId,
            Label = dto.Name,
            StartUtc = dto.PlannedStartUtc,
            EndUtc = dto.PlannedEndUtc,
            ActualEndUtc = dto.ActualEndUtc,
            IsOverdue = dto.PlannedEndUtc < DateTimeOffset.UtcNow && dto.ActualEndUtc is null && !IsTerminalStatus(dto.Status),
            Status = dto.Status,
        };

    /// <summary>Projects a full Safety Project detail DTO into a display-ready detail view model.</summary>
    /// <param name="dto">The wire DTO from the Project detail endpoint.</param>
    public static ProjectDetailViewModel ToDetailViewModel(this SafetyProjectDto dto)
        => new()
        {
            ProjectId = dto.ProjectId,
            Name = dto.Name,
            Description = dto.Description,
            Status = dto.Status,
            StatusIndicatorState = ToStatusIndicatorState(dto.Status),
            Priority = dto.Priority,
            ProjectOwnerId = dto.ProjectOwnerId,
            ProjectManagerId = dto.ProjectManagerId,
            PlannedStartUtc = dto.PlannedStartUtc,
            PlannedEndUtc = dto.PlannedEndUtc,
            ActualStartUtc = dto.ActualStartUtc,
            ActualEndUtc = dto.ActualEndUtc,
            PlannedBudget = dto.PlannedBudget,
            OpenMilestoneCount = dto.OpenMilestoneCount,
            LastModifiedAtUtc = dto.LastModifiedAtUtc,
            IsOverdue = dto.PlannedEndUtc < DateTimeOffset.UtcNow && dto.ActualEndUtc is null && !IsTerminalStatus(dto.Status),
        };

    /// <summary>Maps a raw lifecycle status code to the closest StatusIndicatorComponent operational state.</summary>
    /// <param name="status">The raw status code (case-insensitive; covers both the legacy ProjectStatus and SafetyProject lifecycle vocabularies).</param>
    public static string ToStatusIndicatorState(string status) => status.Trim().ToLowerInvariant() switch
    {
        "completed" or "active" or "inprogress" or "planned" => "Healthy",
        "planning" or "draft" => "Warning",
        "onhold" or "blocked" => "Warning",
        "cancelled" or "archived" => "Offline",
        _ => "Offline",
    };

    /// <summary>Determines whether a raw lifecycle status code represents a terminal (closed) state.</summary>
    /// <param name="status">The raw status code.</param>
    private static bool IsTerminalStatus(string status) => status.Trim().ToLowerInvariant() switch
    {
        "completed" or "cancelled" or "archived" => true,
        _ => false,
    };
}
