// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Repository abstraction for the AccumulationArea read-model.
// SchemaVersion: 1

using PlantSafety.Application.HazardousWasteManagement.AccumulationAreas.Models;

namespace PlantSafety.Application.HazardousWasteManagement.AccumulationAreas.Repositories;

/// <summary>Read-only repository for AccumulationArea data. Implemented in the Infrastructure layer.</summary>
public interface IAccumulationAreaRepository
{
    /// <summary>Returns all accumulation areas with at least one open container nearing its regulatory time limit.</summary>
    Task<IReadOnlyList<AccumulationAreaItemDto>> ListAccumulationAreasNearingTimeLimitAsync(
        Guid tenantId, Guid? siteId, CancellationToken cancellationToken);
}
