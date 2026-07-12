// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Repository abstraction for the Internal read-model.
// SchemaVersion: 1
using PlantSafety.Application.Integrations.Notifications.Internal.Models;

namespace PlantSafety.Application.Integrations.Notifications.Internal.Repositories;

/// <summary>Read-only repository for Internal data. Implemented in the Infrastructure layer.</summary>
public interface IInternalRepository
{
    /// <summary>Returns Internal items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<InternalItemDto>> GetInternalNotificationsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}