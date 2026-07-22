// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the WorkOrderPriority read-model.
// SchemaVersion: 1

using PlantSafety.Application.Scheduling.WorkOrderPriority.Models;

namespace PlantSafety.Application.Scheduling.WorkOrderPriority.Repositories;

/// <summary>Read-only repository for WorkOrderPriority data. Implemented in the Infrastructure layer.</summary>
public interface IWorkOrderPriorityRepository
{
    /// <summary>Returns WorkOrderPriority items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<WorkOrderPriorityItemDto>> GetWorkOrderPriorityAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}