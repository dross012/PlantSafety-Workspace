// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Reporting read-model.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.Reporting.Models;

namespace PlantSafety.Application.Analytics.Reporting.Repositories;

/// <summary>Read-only repository for Reporting data. Implemented in the Infrastructure layer.</summary>
public interface IReportingRepository
{
    /// <summary>Returns Reporting items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ReportingItemDto>> GetReportItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
