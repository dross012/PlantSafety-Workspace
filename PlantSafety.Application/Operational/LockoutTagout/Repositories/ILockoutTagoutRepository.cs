// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the LockoutTagout read-model.
// SchemaVersion: 1

using PlantSafety.Application.Operational.LockoutTagout.Models;

namespace PlantSafety.Application.Operational.LockoutTagout.Repositories;

/// <summary>Read-only repository for LockoutTagout data. Implemented in the Infrastructure layer.</summary>
public interface ILockoutTagoutRepository
{
    /// <summary>Returns LockoutTagout items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<LockoutTagoutItemDto>> GetLockoutTagoutAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
