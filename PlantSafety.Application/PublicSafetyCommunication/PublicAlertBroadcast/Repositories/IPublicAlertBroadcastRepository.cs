// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Repository abstraction for the PublicAlertBroadcast read-model.
// SchemaVersion: 1

using PlantSafety.Application.PublicSafetyCommunication.PublicAlertBroadcast.Models;

namespace PlantSafety.Application.PublicSafetyCommunication.PublicAlertBroadcast.Repositories;

/// <summary>Read-only repository for PublicAlertBroadcast data. Implemented in the Infrastructure layer.</summary>
public interface IPublicAlertBroadcastRepository
{
    /// <summary>Returns active (Sent/Delivered) PublicAlertBroadcast items for the tenant/site.</summary>
    Task<IReadOnlyList<PublicAlertBroadcastItemDto>> ListActiveBroadcastsAsync(
        Guid tenantId,
        Guid? siteId,
        CancellationToken cancellationToken);
}
