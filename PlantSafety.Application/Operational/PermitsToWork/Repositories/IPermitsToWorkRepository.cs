// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the PermitsToWork read-model.
// SchemaVersion: 1

using PlantSafety.Application.Operational.PermitsToWork.Models;

namespace PlantSafety.Application.Operational.PermitsToWork.Repositories;

/// <summary>Read-only repository for PermitsToWork data. Implemented in the Infrastructure layer.</summary>
public interface IPermitsToWorkRepository
{
    /// <summary>Returns PermitsToWork items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<PermitsToWorkItemDto>> GetPermitsToWorkAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
