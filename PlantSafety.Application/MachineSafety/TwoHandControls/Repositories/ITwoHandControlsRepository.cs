// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the TwoHandControls read-model.
// SchemaVersion: 1

using PlantSafety.Application.MachineSafety.TwoHandControls.Models;

namespace PlantSafety.Application.MachineSafety.TwoHandControls.Repositories;

/// <summary>Read-only repository for TwoHandControls data. Implemented in the Infrastructure layer.</summary>
public interface ITwoHandControlsRepository
{
    /// <summary>Returns TwoHandControls items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<TwoHandControlsItemDto>> GetTwoHandControlsItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}