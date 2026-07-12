// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the WorkatHeight read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Permits.WorkatHeight.Models;

namespace PlantSafety.Application.Compliance.Permits.WorkatHeight.Repositories;

/// <summary>Read-only repository for WorkatHeight data. Implemented in the Infrastructure layer.</summary>
public interface IWorkatHeightRepository
{
    /// <summary>Returns WorkatHeight items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<WorkatHeightItemDto>> GetWorkatHeightAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
