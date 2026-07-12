// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the RiskManagement read-model.
// SchemaVersion: 1

using PlantSafety.Application.Operational.RiskManagement.Models;

namespace PlantSafety.Application.Operational.RiskManagement.Repositories;

/// <summary>Read-only repository for RiskManagement data. Implemented in the Infrastructure layer.</summary>
public interface IRiskManagementRepository
{
    /// <summary>Returns RiskManagement items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<RiskManagementItemDto>> GetRiskManagementAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
