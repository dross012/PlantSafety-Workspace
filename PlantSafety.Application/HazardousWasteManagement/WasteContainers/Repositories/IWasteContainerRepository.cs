// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Repository abstraction for the WasteContainer read-model.
// SchemaVersion: 1

using PlantSafety.Application.HazardousWasteManagement.WasteContainers.Models;

namespace PlantSafety.Application.HazardousWasteManagement.WasteContainers.Repositories;

/// <summary>Read-only repository for WasteContainer data. Implemented in the Infrastructure layer.</summary>
public interface IWasteContainerRepository
{
    /// <summary>Returns all WasteContainer items scoped to the given accumulation area.</summary>
    Task<IReadOnlyList<WasteContainerItemDto>> ListContainersByAccumulationAreaAsync(
        Guid tenantId, Guid accumulationAreaId, CancellationToken cancellationToken);
}
