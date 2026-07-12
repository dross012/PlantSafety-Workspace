// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the CultureIndex read-model.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.Culture.CultureIndex.Models;

namespace PlantSafety.Application.Analytics.Culture.CultureIndex.Repositories;

/// <summary>Read-only repository for CultureIndex data. Implemented in the Infrastructure layer.</summary>
public interface ICultureIndexRepository
{
    /// <summary>Returns CultureIndex items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<CultureIndexItemDto>> GetCultureIndexSnapshotsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
