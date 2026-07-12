// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Restore read-model.
// SchemaVersion: 1

using PlantSafety.Application.CrossDomain.Restore.Models;

namespace PlantSafety.Application.CrossDomain.Restore.Repositories;

/// <summary>Read-only repository for Restore data. Implemented in the Infrastructure layer.</summary>
public interface IRestoreRepository
{
    /// <summary>Returns Restore items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<RestoreItemDto>> GetRestoreJobsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
