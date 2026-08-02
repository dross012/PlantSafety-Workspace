// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// UPDATED: 2026-08-02 -- Phase 4 full build-out: added GetByIdAsync, backing the new
//          GetObservationByIdQuery read path via usp_ObservationItems_GetById.
// FILE DESCRIPTION: Repository abstraction for the Observation read-model.
// SchemaVersion: 1

using PlantSafety.Application.SafetyEvents.Observation.Models;

namespace PlantSafety.Application.SafetyEvents.Observation.Repositories;

/// <summary>Read-only repository for Observation data. Implemented in the Infrastructure layer.</summary>
public interface IObservationRepository
{
    /// <summary>Returns Observation items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ObservationItemDto>> GetObservationsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);

    /// <summary>
    /// Returns the Observation read-model record for the given identity, scoped to the given
    /// tenant, or <see langword="null"/> if not found or if the record belongs to a different
    /// tenant.
    /// </summary>
    Task<ObservationItemDto?> GetByIdAsync(Guid observationId, Guid tenantId, CancellationToken cancellationToken);
}
