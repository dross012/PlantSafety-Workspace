// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the ConfinedSpace read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Permits.ConfinedSpace.Models;

namespace PlantSafety.Application.Compliance.Permits.ConfinedSpace.Repositories;

/// <summary>Read-only repository for ConfinedSpace data. Implemented in the Infrastructure layer.</summary>
public interface IConfinedSpaceRepository
{
    /// <summary>Returns ConfinedSpace items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ConfinedSpaceItemDto>> GetConfinedSpaceAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
