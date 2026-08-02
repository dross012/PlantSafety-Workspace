// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the WorkOrderPriority read-model.
// SchemaVersion: 1
// UPDATED: 2026-07-30 -- Added GetByIdAsync (Phase 4 Scheduling.WorkOrderPriority remediation),
//   closing the gap where an API client had no way to read a single WorkOrderPriority record's
//   current state (and thus obtain its ETag before writing) by identity. Tenant-scoped at the DB
//   boundary, mirroring
//   PlantSafety.Application.RiskManagement.Repositories.IRiskRepository.GetByIdAsync.

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

    /// <summary>
    /// Returns the WorkOrderPriority read-model record for the given identity, scoped to the given
    /// tenant, or <see langword="null"/> if not found or if the record belongs to a different
    /// tenant.
    /// </summary>
    Task<WorkOrderPriorityItemDto?> GetByIdAsync(Guid workOrderPriorityId, Guid tenantId, CancellationToken cancellationToken);
}