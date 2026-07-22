// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the EmergencyDrillPlanning read-model.
// SchemaVersion: 1

using PlantSafety.Application.Scheduling.EmergencyDrillPlanning.Models;

namespace PlantSafety.Application.Scheduling.EmergencyDrillPlanning.Repositories;

/// <summary>Read-only repository for EmergencyDrillPlanning data. Implemented in the Infrastructure layer.</summary>
public interface IEmergencyDrillPlanningRepository
{
    /// <summary>Returns EmergencyDrillPlanning items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<EmergencyDrillPlanningItemDto>> GetEmergencyDrillPlanningAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}