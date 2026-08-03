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

    /// <summary>
    /// Returns a deterministically ordered, paginated page of the report catalog for the
    /// tenant, optionally scoped to a single site and/or filtered by report type. Backs the
    /// PlantSafety.UI Reporting module's Reports List page.
    /// </summary>
    Task<(IReadOnlyList<ReportListItemDto> Items, int TotalCount)> GetReportsPagedAsync(
        Guid tenantId,
        Guid? siteId,
        string? typeFilter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);

    /// <summary>
    /// Returns a single report's full detail by identifier, or <see langword="null"/> if not
    /// found for the tenant. Backs the PlantSafety.UI Reporting module's Report Detail page.
    /// </summary>
    Task<ReportDetailDto?> GetReportByIdAsync(
        Guid tenantId,
        Guid reportId,
        CancellationToken cancellationToken);
}
