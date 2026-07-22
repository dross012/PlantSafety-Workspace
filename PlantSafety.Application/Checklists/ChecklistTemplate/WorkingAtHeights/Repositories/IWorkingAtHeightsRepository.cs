// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the WorkingAtHeights read-model. Implemented in the Infrastructure layer (Dapper/stored-procedure-backed; not generated in this C#-only pass).
// SchemaVersion: 1

using PlantSafety.Application.Checklists.ChecklistTemplate.WorkingAtHeights.Models;

namespace PlantSafety.Application.Checklists.ChecklistTemplate.WorkingAtHeights.Repositories;

/// <summary>Read-only repository for WorkingAtHeights data. Implemented in the Infrastructure layer.</summary>
public interface IWorkingAtHeightsRepository
{
    /// <summary>Returns WorkingAtHeights items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<WorkingAtHeightsItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}