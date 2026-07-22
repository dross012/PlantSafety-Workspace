// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-only repository abstraction for the Alert read-model, used by queries.
//   Implemented in the Infrastructure layer (Dapper/stored-procedure-only; not included in this
//   Domain+Application-only generation pass).
// SchemaVersion: 1

using PlantSafety.Application.Alerts.Models;

namespace PlantSafety.Application.Alerts.Repositories;

/// <summary>Read-only repository for Alert data. Implemented in the Infrastructure layer.</summary>
public interface IAlertQueryRepository
{
    /// <summary>Retrieves a single Alert by identifier, or <see langword="null"/> if not found.</summary>
    Task<AlertDto?> GetByIdAsync(Guid tenantId, Guid alertId, CancellationToken cancellationToken);

    /// <summary>Returns all Alerts not yet closed, optionally scoped to a single site.</summary>
    Task<IReadOnlyList<AlertDto>> GetActiveAlertsAsync(Guid tenantId, Guid? siteId, CancellationToken cancellationToken);

    /// <summary>Returns Alerts within the given catalog category, optionally scoped to a single site.</summary>
    Task<IReadOnlyList<AlertDto>> GetAlertsByCategoryAsync(Guid tenantId, Guid? siteId, string categoryCode, CancellationToken cancellationToken);

    /// <summary>
    /// Returns a deterministically ordered, paginated page of Alert history (all statuses) within
    /// the given UTC date range, for "14. Alert Logging &amp; Reporting".
    /// </summary>
    Task<(IReadOnlyList<AlertDto> Items, int TotalCount)> GetAlertHistoryAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, int pageNumber, int pageSize, CancellationToken cancellationToken);

    /// <summary>Returns the recorded multi-channel delivery attempts for a single Alert.</summary>
    Task<IReadOnlyList<AlertDeliveryRecordDto>> GetDeliveryRecordsAsync(Guid alertId, CancellationToken cancellationToken);

    /// <summary>Returns the recorded response actions for a single Alert.</summary>
    Task<IReadOnlyList<AlertResponseActionDto>> GetResponseActionsAsync(Guid alertId, CancellationToken cancellationToken);
}
