// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Repository abstraction for the IncidentNotification read-model.
// SchemaVersion: 1

using PlantSafety.Application.PublicSafetyCommunication.IncidentNotificationDispatch.Models;

namespace PlantSafety.Application.PublicSafetyCommunication.IncidentNotificationDispatch.Repositories;

/// <summary>Read-only repository for IncidentNotification data. Implemented in the Infrastructure layer.</summary>
public interface IIncidentNotificationRepository
{
    /// <summary>Returns IncidentNotification items for the tenant/site/incident within the given UTC date range.</summary>
    Task<IReadOnlyList<IncidentNotificationItemDto>> GetIncidentNotificationStatusAsync(
        Guid tenantId,
        Guid? siteId,
        Guid? incidentId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
