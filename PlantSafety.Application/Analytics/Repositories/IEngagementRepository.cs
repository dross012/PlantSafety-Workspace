// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-13
// FILE DESCRIPTION: Repository abstraction for the Engagement analytics read-model.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.Models;

namespace PlantSafety.Application.Analytics.Repositories;

/// <summary>Read-only repository for Engagement analytics data. Implemented in the Infrastructure layer.</summary>
public interface IEngagementRepository
{
    /// <summary>Returns Engagement analytics data points for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<EngagementDataPointDto>> GetDataPointsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
