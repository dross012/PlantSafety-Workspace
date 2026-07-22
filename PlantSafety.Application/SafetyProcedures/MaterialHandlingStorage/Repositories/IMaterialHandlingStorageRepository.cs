// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the MaterialHandlingStorage read-model.
// SchemaVersion: 1

using PlantSafety.Application.SafetyProcedures.MaterialHandlingStorage.Models;

namespace PlantSafety.Application.SafetyProcedures.MaterialHandlingStorage.Repositories;

/// <summary>Read-only repository for MaterialHandlingStorage data. Implemented in the Infrastructure layer.</summary>
public interface IMaterialHandlingStorageRepository
{
    /// <summary>Returns MaterialHandlingStorage items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<MaterialHandlingStorageItemDto>> GetMaterialHandlingStorageItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}