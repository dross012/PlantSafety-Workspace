// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the SiteOrientation read-model.
// SchemaVersion: 1

using PlantSafety.Application.ContractorSafety.SiteOrientation.Models;

namespace PlantSafety.Application.ContractorSafety.SiteOrientation.Repositories;

/// <summary>Read-only repository for SiteOrientation data. Implemented in the Infrastructure layer.</summary>
public interface ISiteOrientationRepository
{
    /// <summary>Returns SiteOrientation items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<SiteOrientationItemDto>> GetSiteOrientationItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}