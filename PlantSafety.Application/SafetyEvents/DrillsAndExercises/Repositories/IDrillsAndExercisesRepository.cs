// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the DrillsAndExercises read-model.
// SchemaVersion: 1

using PlantSafety.Application.SafetyEvents.DrillsAndExercises.Models;

namespace PlantSafety.Application.SafetyEvents.DrillsAndExercises.Repositories;

/// <summary>Read-only repository for DrillsAndExercises data. Implemented in the Infrastructure layer.</summary>
public interface IDrillsAndExercisesRepository
{
    /// <summary>Returns DrillsAndExercises items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<DrillsAndExercisesItemDto>> GetDrillsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
