// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Repository abstraction for the Integrations.Reporting read-model.
// SchemaVersion: 1

using PlantSafety.Application.Integrations.Reporting.Models;

namespace PlantSafety.Application.Integrations.Reporting.Repositories;

/// <summary>Read-only repository for Integrations.Reporting data. Implemented in the Infrastructure layer.</summary>
public interface IReportingRepository
{
    /// <summary>Returns Integrations.Reporting report delivery items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ReportingItemDto>> GetReportDeliveriesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
