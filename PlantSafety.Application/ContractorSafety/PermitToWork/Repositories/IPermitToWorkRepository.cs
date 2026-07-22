// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the PermitToWork read-model.
// SchemaVersion: 1

using PlantSafety.Application.ContractorSafety.PermitToWork.Models;

namespace PlantSafety.Application.ContractorSafety.PermitToWork.Repositories;

/// <summary>Read-only repository for PermitToWork data. Implemented in the Infrastructure layer.</summary>
public interface IPermitToWorkRepository
{
    /// <summary>Returns PermitToWork items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<PermitToWorkItemDto>> GetPermitToWorkItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}