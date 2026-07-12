// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Checklists read-model.
// SchemaVersion: 1

using PlantSafety.Application.Operational.Checklists.Models;

namespace PlantSafety.Application.Operational.Checklists.Repositories;

/// <summary>Read-only repository for Checklists data. Implemented in the Infrastructure layer.</summary>
public interface IChecklistsRepository
{
    /// <summary>Returns Checklists items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ChecklistsItemDto>> GetChecklistsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
