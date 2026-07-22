// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the EmergencyResponsePlanning read-model.
// SchemaVersion: 1

using PlantSafety.Application.SafetyProcedures.EmergencyResponsePlanning.Models;

namespace PlantSafety.Application.SafetyProcedures.EmergencyResponsePlanning.Repositories;

/// <summary>Read-only repository for EmergencyResponsePlanning data. Implemented in the Infrastructure layer.</summary>
public interface IEmergencyResponsePlanningRepository
{
    /// <summary>Returns EmergencyResponsePlanning items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<EmergencyResponsePlanningItemDto>> GetEmergencyResponsePlanningItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}