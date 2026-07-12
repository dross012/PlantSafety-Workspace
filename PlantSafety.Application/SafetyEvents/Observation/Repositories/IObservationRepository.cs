// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
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
}
