// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the TrendModeling read-model.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.Culture.TrendModeling.Models;

namespace PlantSafety.Application.Analytics.Culture.TrendModeling.Repositories;

/// <summary>Read-only repository for TrendModeling data. Implemented in the Infrastructure layer.</summary>
public interface ITrendModelingRepository
{
    /// <summary>Returns TrendModeling items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<TrendModelingItemDto>> GetTrendModelsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
