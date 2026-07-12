// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Radiography read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Permits.Radiography.Models;

namespace PlantSafety.Application.Compliance.Permits.Radiography.Repositories;

/// <summary>Read-only repository for Radiography data. Implemented in the Infrastructure layer.</summary>
public interface IRadiographyRepository
{
    /// <summary>Returns Radiography items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<RadiographyItemDto>> GetRadiographyAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
