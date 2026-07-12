// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the FatigueManagement read-model.
// SchemaVersion: 1

using PlantSafety.Application.Operational.FatigueManagement.Models;

namespace PlantSafety.Application.Operational.FatigueManagement.Repositories;

/// <summary>Read-only repository for FatigueManagement data. Implemented in the Infrastructure layer.</summary>
public interface IFatigueManagementRepository
{
    /// <summary>Returns FatigueManagement items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<FatigueManagementItemDto>> GetFatigueManagementAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
