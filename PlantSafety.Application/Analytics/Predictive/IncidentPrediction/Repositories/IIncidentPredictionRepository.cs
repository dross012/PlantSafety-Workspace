// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the IncidentPrediction read-model.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.Predictive.IncidentPrediction.Models;

namespace PlantSafety.Application.Analytics.Predictive.IncidentPrediction.Repositories;

/// <summary>Read-only repository for IncidentPrediction data. Implemented in the Infrastructure layer.</summary>
public interface IIncidentPredictionRepository
{
    /// <summary>Returns IncidentPrediction items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<IncidentPredictionItemDto>> GetIncidentPredictionsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
