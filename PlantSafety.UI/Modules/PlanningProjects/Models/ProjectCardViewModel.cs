// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Display-ready view model for a single project card
//   rendered by PlanCard.razor on the Planning Projects, Project Budgeting,
//   and Project Scheduling dashboard pages. Produced by
//   PlanningProjectsMappingService — never constructed directly from the
//   wire DTO inside a page/component (Blazor UI rule: view models are
//   display-shaped, DTOs are wire-shaped).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;

namespace PlantSafety.UI.Modules.PlanningProjects.Models;

/// <summary>Display-ready summary of a single planning project for card/grid rendering.</summary>
public sealed record ProjectCardViewModel
{
    /// <summary>Gets the project identifier.</summary>
    public required Guid ProjectId { get; init; }

    /// <summary>Gets the project name.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the raw lifecycle status code (e.g., "InProgress").</summary>
    public required string Status { get; init; }

    /// <summary>Gets the StatusIndicatorComponent operational state derived from <see cref="Status"/>.</summary>
    public required string StatusIndicatorState { get; init; }

    /// <summary>Gets the UTC date this project is targeted to complete.</summary>
    public required DateTime TargetCompletionUtc { get; init; }

    /// <summary>Gets the owning user identifier.</summary>
    public required Guid OwnerId { get; init; }

    /// <summary>Gets the number of whole days remaining until target completion (negative when overdue).</summary>
    public required int DaysRemaining { get; init; }

    /// <summary>Gets a value indicating whether the target completion date has passed without closure.</summary>
    public required bool IsOverdue { get; init; }
}
