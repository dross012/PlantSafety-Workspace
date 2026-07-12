// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Executive read-model.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.Dashboard.Executive.Models;

namespace PlantSafety.Application.Analytics.Dashboard.Executive.Repositories;

/// <summary>Read-only repository for Executive data. Implemented in the Infrastructure layer.</summary>
public interface IExecutiveRepository
{
    /// <summary>Returns Executive items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ExecutiveItemDto>> GetExecutiveSummaryAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
