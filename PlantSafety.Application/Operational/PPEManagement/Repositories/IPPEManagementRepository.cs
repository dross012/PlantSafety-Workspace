// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the PPEManagement read-model.
// SchemaVersion: 1

using PlantSafety.Application.Operational.PPEManagement.Models;

namespace PlantSafety.Application.Operational.PPEManagement.Repositories;

/// <summary>Read-only repository for PPEManagement data. Implemented in the Infrastructure layer.</summary>
public interface IPPEManagementRepository
{
    /// <summary>Returns PPEManagement items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<PPEManagementItemDto>> GetPPEManagementAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
