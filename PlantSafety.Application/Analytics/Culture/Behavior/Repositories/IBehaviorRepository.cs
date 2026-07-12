// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Behavior read-model.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.Culture.Behavior.Models;

namespace PlantSafety.Application.Analytics.Culture.Behavior.Repositories;

/// <summary>Read-only repository for Behavior data. Implemented in the Infrastructure layer.</summary>
public interface IBehaviorRepository
{
    /// <summary>Returns Behavior items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<BehaviorItemDto>> GetBehaviorObservationsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
