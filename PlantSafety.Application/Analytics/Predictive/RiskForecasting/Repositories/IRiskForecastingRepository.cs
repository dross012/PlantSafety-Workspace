// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the RiskForecasting read-model.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.Predictive.RiskForecasting.Models;

namespace PlantSafety.Application.Analytics.Predictive.RiskForecasting.Repositories;

/// <summary>Read-only repository for RiskForecasting data. Implemented in the Infrastructure layer.</summary>
public interface IRiskForecastingRepository
{
    /// <summary>Returns RiskForecasting items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<RiskForecastingItemDto>> GetRiskForecastsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
