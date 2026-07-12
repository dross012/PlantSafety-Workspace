// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the PlanningProjects read-model.
// SchemaVersion: 1

using PlantSafety.Application.PlanningProjects.PlanningProjects.Models;

namespace PlantSafety.Application.PlanningProjects.PlanningProjects.Repositories;

/// <summary>Read-only repository for PlanningProjects data. Implemented in the Infrastructure layer.</summary>
public interface IPlanningProjectsRepository
{
    /// <summary>Returns PlanningProjects items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<PlanningProjectItemDto>> GetProjectsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
