// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the PermitScheduling read-model.
// SchemaVersion: 1

using PlantSafety.Application.PermitToWork.PermitScheduling.Models;

namespace PlantSafety.Application.PermitToWork.PermitScheduling.Repositories;

/// <summary>Read-only repository for PermitScheduling data. Implemented in the Infrastructure layer.</summary>
public interface IPermitSchedulingRepository
{
    /// <summary>Returns PermitScheduling items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<PermitSchedulingItemDto>> GetPermitSchedulingAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}