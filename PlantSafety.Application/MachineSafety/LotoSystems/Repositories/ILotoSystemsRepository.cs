// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the LotoSystems read-model.
// SchemaVersion: 1

using PlantSafety.Application.MachineSafety.LotoSystems.Models;

namespace PlantSafety.Application.MachineSafety.LotoSystems.Repositories;

/// <summary>Read-only repository for LotoSystems data. Implemented in the Infrastructure layer.</summary>
public interface ILotoSystemsRepository
{
    /// <summary>Returns LotoSystems items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<LotoSystemsItemDto>> GetLotoSystemsItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}