// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Roles read-model.
// SchemaVersion: 1

using PlantSafety.Application.CrossDomain.Roles.Models;

namespace PlantSafety.Application.CrossDomain.Roles.Repositories;

/// <summary>Read-only repository for Roles data. Implemented in the Infrastructure layer.</summary>
public interface IRolesRepository
{
    /// <summary>Returns Roles items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<RolesItemDto>> GetRolesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
