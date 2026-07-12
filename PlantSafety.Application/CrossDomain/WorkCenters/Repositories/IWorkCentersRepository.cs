// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the WorkCenters read-model.
// SchemaVersion: 1

using PlantSafety.Application.CrossDomain.WorkCenters.Models;

namespace PlantSafety.Application.CrossDomain.WorkCenters.Repositories;

/// <summary>Read-only repository for WorkCenters data. Implemented in the Infrastructure layer.</summary>
public interface IWorkCentersRepository
{
    /// <summary>Returns WorkCenters items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<WorkCentersItemDto>> GetWorkCentersAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
