// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the NearMisses read-model.
// SchemaVersion: 1

using PlantSafety.Application.SafetyEvents.NearMisses.Models;

namespace PlantSafety.Application.SafetyEvents.NearMisses.Repositories;

/// <summary>Read-only repository for NearMisses data. Implemented in the Infrastructure layer.</summary>
public interface INearMissesRepository
{
    /// <summary>Returns NearMisses items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<NearMissesItemDto>> GetNearMissesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
