// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Display-ready view model for a single project row on the
//   Project Budgeting dashboard. Produced by ProjectBudgetingUiService from
//   the shared PlanningProjectListItemDto plus (when available) an enriching
//   SafetyProjectDto detail fetch for the planned-budget figure, since the
//   PlanningProjects list endpoint does not itself carry budget data.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;

namespace PlantSafety.UI.Modules.ProjectBudgeting.Models;

/// <summary>Display-ready budget row for the Project Budgeting dashboard.</summary>
public sealed record ProjectBudgetViewModel
{
    /// <summary>Gets the project identifier.</summary>
    public required Guid ProjectId { get; init; }

    /// <summary>Gets the project name.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the raw lifecycle status code.</summary>
    public required string Status { get; init; }

    /// <summary>Gets the StatusIndicatorComponent operational state derived from <see cref="Status"/>.</summary>
    public required string StatusIndicatorState { get; init; }

    /// <summary>Gets the planned budget, or null when the detail enrichment call failed or has not completed.</summary>
    public decimal? PlannedBudget { get; init; }

    /// <summary>Gets a value indicating whether the budget-enrichment detail fetch succeeded for this row.</summary>
    public required bool IsBudgetKnown { get; init; }

    /// <summary>Gets the UTC date this project is targeted to complete.</summary>
    public required DateTime TargetCompletionUtc { get; init; }

    /// <summary>Gets the owning user identifier.</summary>
    public required Guid OwnerId { get; init; }
}
