// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Repository abstraction for the Integrations.Analytics read-model.
// SchemaVersion: 1

using PlantSafety.Application.Integrations.Analytics.Models;

namespace PlantSafety.Application.Integrations.Analytics.Repositories;

/// <summary>Read-only repository for Integrations.Analytics data. Implemented in the Infrastructure layer.</summary>
public interface IAnalyticsRepository
{
    /// <summary>Returns Integrations.Analytics items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<AnalyticsItemDto>> GetRecordsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
