// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-only Application-layer repository abstraction for the CommunityNotification read-model.
// SchemaVersion: 1

using PlantSafety.Application.CommunitySafety.CommunityNotification.Models;

namespace PlantSafety.Application.CommunitySafety.CommunityNotification.Repositories;

/// <summary>Read-only repository for CommunityNotification data. Implemented in the Infrastructure layer.</summary>
public interface ICommunityNotificationRepository
{
    /// <summary>Returns CommunityNotification items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<CommunityNotificationItemDto>> GetCommunityNotificationItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
