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

    /// <summary>
    /// Returns the PredictiveAnalytics read-model record for the given identity, scoped to the
    /// given tenant, or <see langword="null"/> if not found or if the record belongs to a
    /// different tenant. Added 2026-08-02 for the Phase 4 full build-out (backs
    /// GetPredictiveAnalyticsByIdQuery); additive, does not affect the pre-existing
    /// GetPredictionsAsync member.
    /// </summary>
    Task<PredictiveAnalyticsItemDto?> GetByIdAsync(Guid predictionId, Guid tenantId, CancellationToken cancellationToken);
}
