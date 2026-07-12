// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the HazardousMaterials read-model.
// SchemaVersion: 1

using PlantSafety.Application.Operational.HazardousMaterials.Models;

namespace PlantSafety.Application.Operational.HazardousMaterials.Repositories;

/// <summary>Read-only repository for HazardousMaterials data. Implemented in the Infrastructure layer.</summary>
public interface IHazardousMaterialsRepository
{
    /// <summary>Returns HazardousMaterials items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<HazardousMaterialsItemDto>> GetHazardousMaterialsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
