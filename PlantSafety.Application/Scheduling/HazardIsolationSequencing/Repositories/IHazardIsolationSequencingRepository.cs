// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the HazardIsolationSequencing read-model.
// SchemaVersion: 1

using PlantSafety.Application.Scheduling.HazardIsolationSequencing.Models;

namespace PlantSafety.Application.Scheduling.HazardIsolationSequencing.Repositories;

/// <summary>Read-only repository for HazardIsolationSequencing data. Implemented in the Infrastructure layer.</summary>
public interface IHazardIsolationSequencingRepository
{
    /// <summary>Returns HazardIsolationSequencing items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<HazardIsolationSequencingItemDto>> GetHazardIsolationSequencingAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}