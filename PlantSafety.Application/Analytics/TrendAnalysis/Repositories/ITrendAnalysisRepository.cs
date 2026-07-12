// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the TrendAnalysis read-model.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.TrendAnalysis.Models;

namespace PlantSafety.Application.Analytics.TrendAnalysis.Repositories;

/// <summary>Read-only repository for TrendAnalysis data. Implemented in the Infrastructure layer.</summary>
public interface ITrendAnalysisRepository
{
    /// <summary>Returns TrendAnalysis items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<TrendAnalysisItemDto>> GetTrendPointsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
