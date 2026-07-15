// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Display-ready view model for a single project milestone
//   row rendered by MilestoneList.razor. NOTE: PlantSafety.Api does not yet
//   expose a per-project milestone list endpoint (only
//   SafetyProjectDto.OpenMilestoneCount, an aggregate count) — this type is
//   defined now so MilestoneList.razor has a stable contract to render
//   against the day a GET /planning-projects/projects/{id}/milestones
//   endpoint ships, per the Blazor UI graceful-degradation rule.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;

namespace PlantSafety.UI.Modules.PlanningProjects.Models;

/// <summary>Display-ready milestone row for the MilestoneList component.</summary>
public sealed record MilestoneViewModel
{
    /// <summary>Gets the milestone identifier.</summary>
    public required Guid MilestoneId { get; init; }

    /// <summary>Gets the milestone name.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the target completion date (UTC).</summary>
    public required DateTime TargetDateUtc { get; init; }

    /// <summary>Gets a value indicating whether the milestone has been achieved.</summary>
    public required bool IsAchieved { get; init; }
}
