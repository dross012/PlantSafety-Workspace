// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: UI-layer mapping service that projects the shared
//   PlanningProjectListItemDto (enriched with an optional planned-budget
//   figure) into a display-ready ProjectBudgetViewModel for the Project
//   Budgeting dashboard. See PlanningProjectsMappingService header for the
//   Mapster-convention rationale.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using PlantSafety.UI.Modules.PlanningProjects.Mappings;
using PlantSafety.UI.Modules.PlanningProjects.Models;
using PlantSafety.UI.Modules.ProjectBudgeting.Models;

namespace PlantSafety.UI.Modules.ProjectBudgeting.Mappings;

/// <summary>Pure, stateless DTO -&gt; ViewModel projections for the Project Budgeting module.</summary>
public static class ProjectBudgetingMappingService
{
    /// <summary>Projects a list-item DTO plus an optional enrichment budget figure into a budget row view model.</summary>
    /// <param name="dto">The wire DTO from the PlanningProjects list endpoint.</param>
    /// <param name="plannedBudget">The planned budget from the per-project detail enrichment call. May legitimately be null (no budget set) when <paramref name="isBudgetKnown"/> is true.</param>
    /// <param name="isBudgetKnown">True when the per-project detail enrichment fetch succeeded (whether or not it returned a budget value); false when the enrichment fetch itself failed.</param>
    public static ProjectBudgetViewModel ToBudgetViewModel(this PlanningProjectListItemDto dto, decimal? plannedBudget, bool isBudgetKnown)
        => new()
        {
            ProjectId = dto.ProjectId,
            Name = dto.Name,
            Status = dto.Status,
            StatusIndicatorState = PlanningProjectsMappingService.ToStatusIndicatorState(dto.Status),
            PlannedBudget = plannedBudget,
            IsBudgetKnown = isBudgetKnown,
            TargetCompletionUtc = dto.TargetCompletionUtc,
            OwnerId = dto.OwnerId,
        };
}
