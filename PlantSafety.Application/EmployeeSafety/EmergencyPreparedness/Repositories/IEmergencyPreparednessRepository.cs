// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the EmergencyPreparedness read-model.
// SchemaVersion: 1

using PlantSafety.Application.EmployeeSafety.EmergencyPreparedness.Models;

namespace PlantSafety.Application.EmployeeSafety.EmergencyPreparedness.Repositories;

/// <summary>Read-only repository for EmergencyPreparedness data. Implemented in the Infrastructure layer.</summary>
public interface IEmergencyPreparednessRepository
{
    /// <summary>Returns EmergencyPreparedness items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<EmergencyPreparednessItemDto>> GetEmergencyPreparednessItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}