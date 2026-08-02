// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// UPDATED: 2026-08-02 -- Phase 4 full build-out: added GetByIdAsync, backing the new
//          GetDrillsAndExercisesByIdQuery/DrillsAndExercisesDetailDto read-before-write flow, so a
//          caller can retrieve a single record's ETag before issuing a lifecycle command. Extends
//          this pre-existing interface (rather than adding a second, colliding
//          IDrillsAndExercisesRepository under this same Application namespace) per this
//          codebase's IPermitsRepository precedent.
// FILE DESCRIPTION: Repository abstraction for the DrillsAndExercises read-model.
// SchemaVersion: 1

using PlantSafety.Application.SafetyEvents.DrillsAndExercises.Models;

namespace PlantSafety.Application.SafetyEvents.DrillsAndExercises.Repositories;

/// <summary>Read-only repository for DrillsAndExercises data. Implemented in the Infrastructure layer.</summary>
public interface IDrillsAndExercisesRepository
{
    /// <summary>
    /// Returns the DrillsAndExercises read-model record for the given identity, scoped to the
    /// given tenant, or <see langword="null"/> if not found or if it belongs to a different tenant.
    /// </summary>
    Task<DrillsAndExercisesDetailDto?> GetByIdAsync(Guid drillsAndExercisesId, Guid tenantId, CancellationToken cancellationToken);

    /// <summary>Returns DrillsAndExercises items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<DrillsAndExercisesItemDto>> GetDrillsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
