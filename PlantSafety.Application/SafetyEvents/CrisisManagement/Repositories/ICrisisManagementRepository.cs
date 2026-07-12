// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the CrisisManagement read-model.
// SchemaVersion: 1

using PlantSafety.Application.SafetyEvents.CrisisManagement.Models;

namespace PlantSafety.Application.SafetyEvents.CrisisManagement.Repositories;

/// <summary>Read-only repository for CrisisManagement data. Implemented in the Infrastructure layer.</summary>
public interface ICrisisManagementRepository
{
    /// <summary>Returns CrisisManagement items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<CrisisManagementItemDto>> GetCrisisEventsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
