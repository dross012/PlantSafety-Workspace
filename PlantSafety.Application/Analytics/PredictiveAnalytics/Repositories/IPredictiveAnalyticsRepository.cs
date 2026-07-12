// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the PredictiveAnalytics read-model.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.PredictiveAnalytics.Models;

namespace PlantSafety.Application.Analytics.PredictiveAnalytics.Repositories;

/// <summary>Read-only repository for PredictiveAnalytics data. Implemented in the Infrastructure layer.</summary>
public interface IPredictiveAnalyticsRepository
{
    /// <summary>Returns PredictiveAnalytics items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<PredictiveAnalyticsItemDto>> GetPredictionsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
