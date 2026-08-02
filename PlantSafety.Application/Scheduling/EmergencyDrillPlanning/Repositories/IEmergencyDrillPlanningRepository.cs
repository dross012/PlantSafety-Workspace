// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the EmergencyDrillPlanning read-model.
// SchemaVersion: 1
// UPDATED: 2026-07-30 -- Phase 4 full build-out: added GetByIdAsync to back
//   GetEmergencyDrillPlanningByIdQuery, mirroring
//   PlantSafety.Application.Compliance.Permits.Repositories.IPermitsRepository.GetByIdAsync.

using PlantSafety.Application.Scheduling.EmergencyDrillPlanning.Models;

namespace PlantSafety.Application.Scheduling.EmergencyDrillPlanning.Repositories;

/// <summary>Read-only repository for EmergencyDrillPlanning data. Implemented in the Infrastructure layer.</summary>
public interface IEmergencyDrillPlanningRepository
{
    /// <summary>
    /// Returns the EmergencyDrillPlanning read-model record for the given identity, scoped to the
    /// given tenant, or <see langword="null"/> if not found or if the record belongs to a
    /// different tenant.
    /// </summary>
    Task<EmergencyDrillPlanningItemDto?> GetByIdAsync(Guid emergencyDrillPlanningId, Guid tenantId, CancellationToken cancellationToken);

    /// <summary>Returns EmergencyDrillPlanning items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<EmergencyDrillPlanningItemDto>> GetEmergencyDrillPlanningAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}