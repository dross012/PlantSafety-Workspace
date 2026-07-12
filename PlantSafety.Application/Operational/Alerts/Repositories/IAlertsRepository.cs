// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Alerts read-model.
// SchemaVersion: 1

using PlantSafety.Application.Operational.Alerts.Models;

namespace PlantSafety.Application.Operational.Alerts.Repositories;

/// <summary>Read-only repository for Alerts data. Implemented in the Infrastructure layer.</summary>
public interface IAlertsRepository
{
    /// <summary>Returns Alerts items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<AlertsItemDto>> GetAlertsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
