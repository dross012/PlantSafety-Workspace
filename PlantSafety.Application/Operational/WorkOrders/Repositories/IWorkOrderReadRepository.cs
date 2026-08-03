// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Application
//  File:           IWorkOrderReadRepository.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the WorkOrder module's query surface.
//                  Implemented in the Infrastructure layer (Dapper). Distinct from
//                  PlantSafety.Domain.Operational.WorkOrders.Interfaces.IWorkOrderRepository (the
//                  write-side aggregate repository). Mirrors the CQRS read/write repository
//                  separation precedent already used by Compliance.Permits and
//                  Operational.PreventiveMaintenance. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Operational.WorkOrders.Models;

namespace PlantSafety.Application.Operational.WorkOrders.Repositories;

/// <summary>Read-only repository for the WorkOrder module's query surface. Implemented in the Infrastructure layer.</summary>
public interface IWorkOrderReadRepository
{
    /// <summary>Returns the WorkOrder read-model record for the given identity, scoped to the given tenant, or null if not found.</summary>
    Task<WorkOrderItemDto?> GetByIdAsync(Guid tenantId, Guid workOrderId, CancellationToken cancellationToken);

    /// <summary>
    /// Returns a paginated page of WorkOrder rows for the given tenant, optionally filtered by
    /// status and/or technician display name (case-insensitive substring match).
    /// </summary>
    Task<(IReadOnlyList<WorkOrderRowDto> Items, int TotalCount)> ListAsync(
        Guid tenantId, string? status, string? technician, int pageNumber, int pageSize, CancellationToken cancellationToken);

    /// <summary>Returns the Work Order Summary card counts for the given tenant.</summary>
    Task<WorkOrderSummaryDto> GetSummaryAsync(Guid tenantId, CancellationToken cancellationToken);

    /// <summary>Returns the Machine Status Summary card counts for the given tenant.</summary>
    Task<MachineStatusSummaryDto> GetMachineStatusSummaryAsync(Guid tenantId, CancellationToken cancellationToken);

    /// <summary>Returns the ordered task steps for a work order.</summary>
    Task<(IReadOnlyList<TaskStepDto> Items, int TotalCount)> GetTasksAsync(Guid tenantId, Guid workOrderId, CancellationToken cancellationToken);

    /// <summary>Returns the parts consumed by a work order.</summary>
    Task<(IReadOnlyList<PartUsageDto> Items, int TotalCount)> GetPartsUsageAsync(Guid tenantId, Guid workOrderId, CancellationToken cancellationToken);

    /// <summary>Returns the chronological event history for a work order.</summary>
    Task<(IReadOnlyList<MaintenanceEventDto> Items, int TotalCount)> GetHistoryAsync(Guid tenantId, Guid workOrderId, CancellationToken cancellationToken);

    /// <summary>Returns the chronological maintenance history for a single machine (across all its work orders).</summary>
    Task<(IReadOnlyList<MaintenanceEventDto> Items, int TotalCount)> GetMachineHistoryAsync(
        Guid tenantId, Guid machineId, int pageNumber, int pageSize, CancellationToken cancellationToken);

    /// <summary>
    /// Returns technician workload cards, aggregated from currently-assigned (non-terminal)
    /// work orders grouped by technician.
    /// </summary>
    Task<(IReadOnlyList<TechnicianWorkloadDto> Items, int TotalCount)> GetTechnicianWorkloadAsync(
        Guid tenantId, int pageNumber, int pageSize, CancellationToken cancellationToken);

    /// <summary>
    /// Returns calendar events within the inclusive UTC date range [<paramref name="rangeStartUtc"/>,
    /// <paramref name="rangeEndUtc"/>], projected from WorkOrder.DueAtUtc.
    /// </summary>
    Task<(IReadOnlyList<MaintenanceCalendarEventDto> Items, int TotalCount)> GetCalendarEventsAsync(
        Guid tenantId, DateTime rangeStartUtc, DateTime rangeEndUtc, CancellationToken cancellationToken);
}
