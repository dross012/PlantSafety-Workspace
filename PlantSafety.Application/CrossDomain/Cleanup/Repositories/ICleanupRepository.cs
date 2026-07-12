// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Cleanup read-model.
// SchemaVersion: 1

using PlantSafety.Application.CrossDomain.Cleanup.Models;

namespace PlantSafety.Application.CrossDomain.Cleanup.Repositories;

/// <summary>Read-only repository for Cleanup data. Implemented in the Infrastructure layer.</summary>
public interface ICleanupRepository
{
    /// <summary>Returns Cleanup items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<CleanupItemDto>> GetCleanupTasksAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
