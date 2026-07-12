// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Lifting read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Permits.Lifting.Models;

namespace PlantSafety.Application.Compliance.Permits.Lifting.Repositories;

/// <summary>Read-only repository for Lifting data. Implemented in the Infrastructure layer.</summary>
public interface ILiftingRepository
{
    /// <summary>Returns Lifting items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<LiftingItemDto>> GetLiftingAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
