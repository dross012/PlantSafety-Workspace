// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the PPEInventory read-model.
// SchemaVersion: 1

using PlantSafety.Application.PPE.PPEInventory.Models;

namespace PlantSafety.Application.PPE.PPEInventory.Repositories;

/// <summary>Read-only repository for PPEInventory data. Implemented in the Infrastructure layer.</summary>
public interface IPPEInventoryRepository
{
    /// <summary>Returns PPEInventory items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<PPEInventoryItemDto>> GetPPEInventoryAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}