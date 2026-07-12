// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Permissions read-model.
// SchemaVersion: 1

using PlantSafety.Application.CrossDomain.Permissions.Models;

namespace PlantSafety.Application.CrossDomain.Permissions.Repositories;

/// <summary>Read-only repository for Permissions data. Implemented in the Infrastructure layer.</summary>
public interface IPermissionsRepository
{
    /// <summary>Returns Permissions items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<PermissionsItemDto>> GetPermissionsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
