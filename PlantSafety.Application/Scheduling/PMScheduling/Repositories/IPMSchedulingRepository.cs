// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the PMScheduling read-model.
// SchemaVersion: 1

using PlantSafety.Application.Scheduling.PMScheduling.Models;

namespace PlantSafety.Application.Scheduling.PMScheduling.Repositories;

/// <summary>Read-only repository for PMScheduling data. Implemented in the Infrastructure layer.</summary>
public interface IPMSchedulingRepository
{
    /// <summary>Returns PMScheduling items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<PMSchedulingItemDto>> GetPMSchedulingAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}