// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Application
//  File:           WorkOrderQueries.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Every query record backing the Maintenance UI's work-order core
//                  (v1/maintenance/work-orders*, machines/*, technicians/*, parts, calendar).
//                  Consolidated into one file per the sibling PreventiveMaintenanceTaskEvents.cs
//                  precedent of grouping closely-related small record types. Plain MediatR
//                  IRequest<Result<T>> records, mirroring
//                  PlantSafety.Application.Operational.PreventiveMaintenance.Queries
//                  .GetPreventiveMaintenanceTaskByIdQuery's style (this module's two reuse points --
//                  GetWorkOrderPredictiveInsightsQuery and GetWorkOrderPreventiveTasksQuery -- both
//                  delegate into that same sibling module's real query surface). Additive-only
//                  evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.WorkOrders.Models;

namespace PlantSafety.Application.Operational.WorkOrders.Queries;

/// <summary>Retrieves a single WorkOrder read-model record by identity, including its current ETag.</summary>
public sealed record GetWorkOrderByIdQuery(Guid TenantId, Guid WorkOrderId) : IRequest<Result<WorkOrderItemDto>>;

/// <summary>Retrieves a paginated page of active work orders, optionally filtered by status/technician.</summary>
public sealed record ListWorkOrdersQuery(
    Guid TenantId,
    int PageNumber,
    int PageSize,
    string? Status,
    string? Technician) : IRequest<Result<PaginatedResult<WorkOrderRowDto>>>;

/// <summary>Retrieves the Work Order Summary card counts.</summary>
public sealed record GetWorkOrderSummaryQuery(Guid TenantId) : IRequest<Result<WorkOrderSummaryDto>>;

/// <summary>Retrieves the Machine Status Summary card counts.</summary>
public sealed record GetMachineStatusSummaryQuery(Guid TenantId) : IRequest<Result<MachineStatusSummaryDto>>;

/// <summary>Retrieves the ordered task steps for a work order.</summary>
public sealed record GetWorkOrderTasksQuery(Guid TenantId, Guid WorkOrderId, int PageNumber, int PageSize)
    : IRequest<Result<PaginatedResult<TaskStepDto>>>;

/// <summary>Retrieves the parts consumed by a work order.</summary>
public sealed record GetWorkOrderPartsUsageQuery(Guid TenantId, Guid WorkOrderId, int PageNumber, int PageSize)
    : IRequest<Result<PaginatedResult<PartUsageDto>>>;

/// <summary>Retrieves the chronological event history for a work order.</summary>
public sealed record GetWorkOrderHistoryQuery(Guid TenantId, Guid WorkOrderId, int PageNumber, int PageSize)
    : IRequest<Result<PaginatedResult<MaintenanceEventDto>>>;

/// <summary>Retrieves the chronological maintenance history for a single machine.</summary>
public sealed record GetMachineHistoryQuery(Guid TenantId, Guid MachineId, int PageNumber, int PageSize)
    : IRequest<Result<PaginatedResult<MaintenanceEventDto>>>;

/// <summary>Retrieves technician workload cards.</summary>
public sealed record GetTechnicianWorkloadQuery(Guid TenantId, int PageNumber, int PageSize)
    : IRequest<Result<PaginatedResult<TechnicianWorkloadDto>>>;

/// <summary>
/// Phase 6 §3.4 Replay/Rebuild Across Systems: reconstructs a WorkOrder's state purely by replaying
/// its <c>[System].[OutboxMessages]</c> domain-event history, optionally cut off at
/// <paramref name="AsOfUtc"/> for point-in-time reconstruction (omit for "as of now").
/// </summary>
public sealed record RebuildWorkOrderTimelineQuery(Guid TenantId, Guid WorkOrderId, DateTime? AsOfUtc = null)
    : IRequest<Result<WorkOrderRebuiltStateDto>>;

/// <summary>Retrieves the parts inventory list.</summary>
public sealed record ListPartsInventoryQuery(Guid TenantId, int PageNumber, int PageSize)
    : IRequest<Result<PaginatedResult<PartInventoryDto>>>;

/// <summary>Retrieves the maintenance calendar events within a date range.</summary>
public sealed record GetMaintenanceCalendarQuery(Guid TenantId, DateOnly RangeStart, DateOnly RangeEnd)
    : IRequest<Result<PaginatedResult<MaintenanceCalendarEventDto>>>;

/// <summary>
/// Reuse point: retrieves fresh predictive-maintenance insights for a work order's target machine
/// by delegating to the real Operational.PredictiveMaintenance module's
/// IPredictiveMaintenanceAlertRepository rather than duplicating prediction logic in this module.
/// </summary>
public sealed record GetWorkOrderPredictiveInsightsQuery(Guid TenantId, Guid WorkOrderId)
    : IRequest<Result<MaintenancePredictiveInsightsDto>>;

/// <summary>
/// Reuse point: retrieves the preventive maintenance task list by delegating to the real
/// Operational.PreventiveMaintenance module's GetPreventiveMaintenanceQuery rather than inventing a
/// second, duplicated preventive-scheduling concept in this module.
/// </summary>
public sealed record GetWorkOrderPreventiveTasksQuery(Guid TenantId, int PageNumber, int PageSize)
    : IRequest<Result<PaginatedResult<PreventiveTaskDto>>>;
