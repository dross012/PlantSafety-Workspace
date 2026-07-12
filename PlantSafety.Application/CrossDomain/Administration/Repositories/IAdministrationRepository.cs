// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Administration read-model.
// SchemaVersion: 1

using PlantSafety.Application.CrossDomain.Administration.Models;

namespace PlantSafety.Application.CrossDomain.Administration.Repositories;

/// <summary>Read-only repository for Administration data. Implemented in the Infrastructure layer.</summary>
public interface IAdministrationRepository
{
    /// <summary>Returns Administration items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<AdministrationItemDto>> GetRecordsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
