// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the ReadModel read-model.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.Dashboard.ReadModel.Models;

namespace PlantSafety.Application.Analytics.Dashboard.ReadModel.Repositories;

/// <summary>Read-only repository for ReadModel data. Implemented in the Infrastructure layer.</summary>
public interface IReadModelRepository
{
    /// <summary>Returns ReadModel items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ReadModelItemDto>> GetDashboardRowsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
