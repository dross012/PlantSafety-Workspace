// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Tracking read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Tracking.Models;

namespace PlantSafety.Application.Compliance.Tracking.Repositories;

/// <summary>Read-only repository for Tracking data. Implemented in the Infrastructure layer.</summary>
public interface ITrackingRepository
{
    /// <summary>Returns Tracking items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<TrackingItemDto>> GetTrackingAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
