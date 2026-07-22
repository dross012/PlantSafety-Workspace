// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the PermitWorkflow read-model.
// SchemaVersion: 1

using PlantSafety.Application.PermitToWork.PermitWorkflow.Models;

namespace PlantSafety.Application.PermitToWork.PermitWorkflow.Repositories;

/// <summary>Read-only repository for PermitWorkflow data. Implemented in the Infrastructure layer.</summary>
public interface IPermitWorkflowRepository
{
    /// <summary>Returns PermitWorkflow items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<PermitWorkflowItemDto>> GetPermitWorkflowAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}