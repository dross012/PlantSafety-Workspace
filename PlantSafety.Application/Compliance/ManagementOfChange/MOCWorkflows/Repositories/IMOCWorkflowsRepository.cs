// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the MOCWorkflows read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.ManagementOfChange.MOCWorkflows.Models;

namespace PlantSafety.Application.Compliance.ManagementOfChange.MOCWorkflows.Repositories;

/// <summary>Read-only repository for MOCWorkflows data. Implemented in the Infrastructure layer.</summary>
public interface IMOCWorkflowsRepository
{
    /// <summary>Returns MOCWorkflows items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<MOCWorkflowsItemDto>> GetMOCWorkflowsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
