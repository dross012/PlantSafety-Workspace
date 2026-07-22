// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the ConfinedSpaceEntry read-model. Implemented in the Infrastructure layer (Dapper/stored-procedure-backed; not generated in this C#-only pass).
// SchemaVersion: 1

using PlantSafety.Application.Checklists.ChecklistTemplate.ConfinedSpaceEntry.Models;

namespace PlantSafety.Application.Checklists.ChecklistTemplate.ConfinedSpaceEntry.Repositories;

/// <summary>Read-only repository for ConfinedSpaceEntry data. Implemented in the Infrastructure layer.</summary>
public interface IConfinedSpaceEntryRepository
{
    /// <summary>Returns ConfinedSpaceEntry items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ConfinedSpaceEntryItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}