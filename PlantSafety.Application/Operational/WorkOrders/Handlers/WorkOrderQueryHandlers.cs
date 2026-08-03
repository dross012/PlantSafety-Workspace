// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Application
//  File:           WorkOrderQueryHandlers.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Handlers for every WorkOrder-native query (list/get/summary/tasks/parts-usage/
//                  history/machine-history/technician-workload/calendar). Consolidated into one
//                  file since all ten depend on the same IWorkOrderReadRepository and share the
//                  identical try/catch-and-log shape -- mirrors the sibling module's practice of
//                  grouping closely related small handlers. The two reuse-point handlers
//                  (predictive insights, preventive tasks) live separately in
//                  WorkOrderReuseQueryHandlers.cs. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Extensions;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.WorkOrders.Models;
using PlantSafety.Application.Operational.WorkOrders.Queries;
using PlantSafety.Application.Operational.WorkOrders.Repositories;

namespace PlantSafety.Application.Operational.WorkOrders.Handlers;

/// <summary>Handles <see cref="GetWorkOrderByIdQuery"/>.</summary>
public sealed class GetWorkOrderByIdQueryHandler(IWorkOrderReadRepository repository, ILogger<GetWorkOrderByIdQueryHandler> logger)
    : IRequestHandler<GetWorkOrderByIdQuery, Result<WorkOrderItemDto>>
{
    /// <inheritdoc/>
    public async Task<Result<WorkOrderItemDto>> Handle(GetWorkOrderByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await repository.GetByIdAsync(request.TenantId, request.WorkOrderId, cancellationToken);
            if (item is null)
            {
                return Result<WorkOrderItemDto>.Failure(new ApplicationError(
                    "Operational.WorkOrders.NotFound",
                    "The requested work order could not be found.",
                    request.WorkOrderId,
                    ErrorSeverity.NotFound));
            }

            return Result<WorkOrderItemDto>.Success(item);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve WorkOrder {WorkOrderId}", request.WorkOrderId);
            return Result<WorkOrderItemDto>.Failure(new ApplicationError(
                "Operational.WorkOrders.RetrievalFailed", "Unable to retrieve work order data at this time.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="ListWorkOrdersQuery"/>.</summary>
public sealed class ListWorkOrdersQueryHandler(IWorkOrderReadRepository repository, ILogger<ListWorkOrdersQueryHandler> logger)
    : IRequestHandler<ListWorkOrdersQuery, Result<PaginatedResult<WorkOrderRowDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<PaginatedResult<WorkOrderRowDto>>> Handle(ListWorkOrdersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var pageNumber = request.PageNumber.ClampPageNumber();
            var pageSize = request.PageSize.ClampPageSize();

            var (items, total) = await repository.ListAsync(request.TenantId, request.Status, request.Technician, pageNumber, pageSize, cancellationToken);

            return Result<PaginatedResult<WorkOrderRowDto>>.Success(new PaginatedResult<WorkOrderRowDto>(items, total, pageNumber, pageSize));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to list WorkOrders for Tenant {TenantId}", request.TenantId);
            return Result<PaginatedResult<WorkOrderRowDto>>.Failure(new ApplicationError(
                "Operational.WorkOrders.ListFailed", "Unable to retrieve work orders at this time.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="GetWorkOrderSummaryQuery"/>.</summary>
public sealed class GetWorkOrderSummaryQueryHandler(IWorkOrderReadRepository repository, ILogger<GetWorkOrderSummaryQueryHandler> logger)
    : IRequestHandler<GetWorkOrderSummaryQuery, Result<WorkOrderSummaryDto>>
{
    /// <inheritdoc/>
    public async Task<Result<WorkOrderSummaryDto>> Handle(GetWorkOrderSummaryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var summary = await repository.GetSummaryAsync(request.TenantId, cancellationToken);
            return Result<WorkOrderSummaryDto>.Success(summary);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to compute WorkOrder summary for Tenant {TenantId}", request.TenantId);
            return Result<WorkOrderSummaryDto>.Failure(new ApplicationError(
                "Operational.WorkOrders.SummaryFailed", "Unable to retrieve the work order summary at this time.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="GetMachineStatusSummaryQuery"/>.</summary>
public sealed class GetMachineStatusSummaryQueryHandler(IWorkOrderReadRepository repository, ILogger<GetMachineStatusSummaryQueryHandler> logger)
    : IRequestHandler<GetMachineStatusSummaryQuery, Result<MachineStatusSummaryDto>>
{
    /// <inheritdoc/>
    public async Task<Result<MachineStatusSummaryDto>> Handle(GetMachineStatusSummaryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var summary = await repository.GetMachineStatusSummaryAsync(request.TenantId, cancellationToken);
            return Result<MachineStatusSummaryDto>.Success(summary);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to compute machine status summary for Tenant {TenantId}", request.TenantId);
            return Result<MachineStatusSummaryDto>.Failure(new ApplicationError(
                "Operational.WorkOrders.MachineStatusSummaryFailed", "Unable to retrieve the machine status summary at this time.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="GetWorkOrderTasksQuery"/>.</summary>
public sealed class GetWorkOrderTasksQueryHandler(IWorkOrderReadRepository repository, ILogger<GetWorkOrderTasksQueryHandler> logger)
    : IRequestHandler<GetWorkOrderTasksQuery, Result<PaginatedResult<TaskStepDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<PaginatedResult<TaskStepDto>>> Handle(GetWorkOrderTasksQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var pageNumber = request.PageNumber.ClampPageNumber();
            var pageSize = request.PageSize.ClampPageSize();

            var (items, total) = await repository.GetTasksAsync(request.TenantId, request.WorkOrderId, cancellationToken);
            return Result<PaginatedResult<TaskStepDto>>.Success(new PaginatedResult<TaskStepDto>(items, total, pageNumber, pageSize));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve tasks for WorkOrder {WorkOrderId}", request.WorkOrderId);
            return Result<PaginatedResult<TaskStepDto>>.Failure(new ApplicationError(
                "Operational.WorkOrders.TasksRetrievalFailed", "Unable to retrieve work order tasks at this time.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="GetWorkOrderPartsUsageQuery"/>.</summary>
public sealed class GetWorkOrderPartsUsageQueryHandler(IWorkOrderReadRepository repository, ILogger<GetWorkOrderPartsUsageQueryHandler> logger)
    : IRequestHandler<GetWorkOrderPartsUsageQuery, Result<PaginatedResult<PartUsageDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<PaginatedResult<PartUsageDto>>> Handle(GetWorkOrderPartsUsageQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var pageNumber = request.PageNumber.ClampPageNumber();
            var pageSize = request.PageSize.ClampPageSize();

            var (items, total) = await repository.GetPartsUsageAsync(request.TenantId, request.WorkOrderId, cancellationToken);
            return Result<PaginatedResult<PartUsageDto>>.Success(new PaginatedResult<PartUsageDto>(items, total, pageNumber, pageSize));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve part usage for WorkOrder {WorkOrderId}", request.WorkOrderId);
            return Result<PaginatedResult<PartUsageDto>>.Failure(new ApplicationError(
                "Operational.WorkOrders.PartsUsageRetrievalFailed", "Unable to retrieve work order part usage at this time.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="GetWorkOrderHistoryQuery"/>.</summary>
public sealed class GetWorkOrderHistoryQueryHandler(IWorkOrderReadRepository repository, ILogger<GetWorkOrderHistoryQueryHandler> logger)
    : IRequestHandler<GetWorkOrderHistoryQuery, Result<PaginatedResult<MaintenanceEventDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<PaginatedResult<MaintenanceEventDto>>> Handle(GetWorkOrderHistoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var pageNumber = request.PageNumber.ClampPageNumber();
            var pageSize = request.PageSize.ClampPageSize();

            var (items, total) = await repository.GetHistoryAsync(request.TenantId, request.WorkOrderId, cancellationToken);
            return Result<PaginatedResult<MaintenanceEventDto>>.Success(new PaginatedResult<MaintenanceEventDto>(items, total, pageNumber, pageSize));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve history for WorkOrder {WorkOrderId}", request.WorkOrderId);
            return Result<PaginatedResult<MaintenanceEventDto>>.Failure(new ApplicationError(
                "Operational.WorkOrders.HistoryRetrievalFailed", "Unable to retrieve work order history at this time.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="GetMachineHistoryQuery"/>.</summary>
public sealed class GetMachineHistoryQueryHandler(IWorkOrderReadRepository repository, ILogger<GetMachineHistoryQueryHandler> logger)
    : IRequestHandler<GetMachineHistoryQuery, Result<PaginatedResult<MaintenanceEventDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<PaginatedResult<MaintenanceEventDto>>> Handle(GetMachineHistoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var pageNumber = request.PageNumber.ClampPageNumber();
            var pageSize = request.PageSize.ClampPageSize();

            var (items, total) = await repository.GetMachineHistoryAsync(request.TenantId, request.MachineId, pageNumber, pageSize, cancellationToken);
            return Result<PaginatedResult<MaintenanceEventDto>>.Success(new PaginatedResult<MaintenanceEventDto>(items, total, pageNumber, pageSize));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve history for Machine {MachineId}", request.MachineId);
            return Result<PaginatedResult<MaintenanceEventDto>>.Failure(new ApplicationError(
                "Operational.WorkOrders.MachineHistoryRetrievalFailed", "Unable to retrieve machine history at this time.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="GetTechnicianWorkloadQuery"/>.</summary>
public sealed class GetTechnicianWorkloadQueryHandler(IWorkOrderReadRepository repository, ILogger<GetTechnicianWorkloadQueryHandler> logger)
    : IRequestHandler<GetTechnicianWorkloadQuery, Result<PaginatedResult<TechnicianWorkloadDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<PaginatedResult<TechnicianWorkloadDto>>> Handle(GetTechnicianWorkloadQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var pageNumber = request.PageNumber.ClampPageNumber();
            var pageSize = request.PageSize.ClampPageSize();

            var (items, total) = await repository.GetTechnicianWorkloadAsync(request.TenantId, pageNumber, pageSize, cancellationToken);
            return Result<PaginatedResult<TechnicianWorkloadDto>>.Success(new PaginatedResult<TechnicianWorkloadDto>(items, total, pageNumber, pageSize));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to compute technician workload for Tenant {TenantId}", request.TenantId);
            return Result<PaginatedResult<TechnicianWorkloadDto>>.Failure(new ApplicationError(
                "Operational.WorkOrders.TechnicianWorkloadFailed", "Unable to retrieve technician workload at this time.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="GetMaintenanceCalendarQuery"/>.</summary>
public sealed class GetMaintenanceCalendarQueryHandler(IWorkOrderReadRepository repository, ILogger<GetMaintenanceCalendarQueryHandler> logger)
    : IRequestHandler<GetMaintenanceCalendarQuery, Result<PaginatedResult<MaintenanceCalendarEventDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<PaginatedResult<MaintenanceCalendarEventDto>>> Handle(GetMaintenanceCalendarQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var rangeStartUtc = request.RangeStart.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var rangeEndUtc = request.RangeEnd.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

            var (items, total) = await repository.GetCalendarEventsAsync(request.TenantId, rangeStartUtc, rangeEndUtc, cancellationToken);
            return Result<PaginatedResult<MaintenanceCalendarEventDto>>.Success(new PaginatedResult<MaintenanceCalendarEventDto>(items, total, 1, Math.Max(items.Count, 1)));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve maintenance calendar for Tenant {TenantId}", request.TenantId);
            return Result<PaginatedResult<MaintenanceCalendarEventDto>>.Failure(new ApplicationError(
                "Operational.WorkOrders.CalendarRetrievalFailed", "Unable to retrieve the maintenance calendar at this time.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="ListPartsInventoryQuery"/>.</summary>
public sealed class ListPartsInventoryQueryHandler(IPartInventoryReadRepository repository, ILogger<ListPartsInventoryQueryHandler> logger)
    : IRequestHandler<ListPartsInventoryQuery, Result<PaginatedResult<PartInventoryDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<PaginatedResult<PartInventoryDto>>> Handle(ListPartsInventoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var pageNumber = request.PageNumber.ClampPageNumber();
            var pageSize = request.PageSize.ClampPageSize();

            var (items, total) = await repository.ListAsync(request.TenantId, pageNumber, pageSize, cancellationToken);
            return Result<PaginatedResult<PartInventoryDto>>.Success(new PaginatedResult<PartInventoryDto>(items, total, pageNumber, pageSize));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to list parts inventory for Tenant {TenantId}", request.TenantId);
            return Result<PaginatedResult<PartInventoryDto>>.Failure(new ApplicationError(
                "Operational.WorkOrders.PartsInventoryListFailed", "Unable to retrieve the parts inventory at this time.", Guid.NewGuid()));
        }
    }
}
