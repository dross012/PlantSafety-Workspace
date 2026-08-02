// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// UPDATED: 2026-08-02 (Phase 4 full build-out) -- added GetByIdAsync, backing the new
//          GetNearMissesByIdQuery read-before-write path for the NearMisses aggregate's lifecycle
//          commands. Distinct from PlantSafety.Domain.SafetyEvents.NearMisses.Interfaces
//          .INearMissesRepository (the write-side aggregate repository) -- same simple name,
//          different namespace, mirroring PlantSafety.Application.Compliance.Permits.Repositories
//          .IPermitsRepository's identical CQRS read/write repository separation precedent.
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

    /// <summary>
    /// Returns the full NearMisses read-model record for the given identity, scoped to the given
    /// tenant, or <see langword="null"/> if not found or if it belongs to a different tenant.
    /// </summary>
    Task<NearMissesDetailDto?> GetByIdAsync(Guid nearMissesId, Guid tenantId, CancellationToken cancellationToken);
}
