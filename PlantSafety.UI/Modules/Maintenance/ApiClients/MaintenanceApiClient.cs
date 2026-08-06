// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the Maintenance module (Volume M1 +
//   Volume MC1 Calendar). Wraps all HTTP communication for the Dashboard,
//   Work Order Detail, Machine History, Preventive Schedule, Technician
//   Workload, Parts Inventory, and Calendar pages. All methods return
//   ApiResponseEnvelope<T>. No HttpClient calls are permitted outside this
//   class for the Maintenance module.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using PlantSafety.UI.Infrastructure.ApiClients;
using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.Maintenance.Models;

namespace PlantSafety.UI.Modules.Maintenance.ApiClients;

/// <summary>
/// Typed API client for the Maintenance module. Components MUST inject this
/// class and call its methods rather than constructing HTTP requests directly.
/// </summary>
public sealed class MaintenanceApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="MaintenanceApiClient"/>.</summary>
    public MaintenanceApiClient(HttpClient http, ILogger<MaintenanceApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves the Work Order Summary card counts.</summary>
    public Task<ApiResponseEnvelope<WorkOrderSummaryDto>> GetWorkOrderSummaryAsync(CancellationToken cancellationToken = default)
        => GetAsync<WorkOrderSummaryDto>("v1/maintenance/work-orders/summary", cancellationToken);

    /// <summary>Retrieves the Machine Status Summary card counts.</summary>
    public Task<ApiResponseEnvelope<MachineStatusSummaryDto>> GetMachineStatusSummaryAsync(CancellationToken cancellationToken = default)
        => GetAsync<MachineStatusSummaryDto>("v1/maintenance/machines/status-summary", cancellationToken);

    /// <summary>Retrieves a paginated page of active work orders.</summary>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<WorkOrderRowDto>>> GetActiveWorkOrdersAsync(
        int pageNumber = 1, int pageSize = 25, string? statusFilter = null, string? technicianFilter = null,
        CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<WorkOrderRowDto>>(
            $"v1/maintenance/work-orders?pageNumber={pageNumber}&pageSize={pageSize}" +
            (string.IsNullOrWhiteSpace(statusFilter) ? string.Empty : $"&status={Uri.EscapeDataString(statusFilter)}") +
            (string.IsNullOrWhiteSpace(technicianFilter) ? string.Empty : $"&technician={Uri.EscapeDataString(technicianFilter)}"),
            cancellationToken);

    /// <summary>Retrieves the full detail for a single work order.</summary>
    public Task<ApiResponseEnvelope<WorkOrderDetailDto>> GetWorkOrderAsync(Guid workOrderId, CancellationToken cancellationToken = default)
        => GetAsync<WorkOrderDetailDto>($"v1/maintenance/work-orders/{workOrderId}", cancellationToken);

    /// <summary>Retrieves the ordered task steps for a work order.</summary>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<TaskStepDto>>> GetTasksAsync(Guid workOrderId, CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<TaskStepDto>>($"v1/maintenance/work-orders/{workOrderId}/tasks", cancellationToken);

    /// <summary>Retrieves the parts consumed by a work order.</summary>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<PartUsageDto>>> GetPartsAsync(Guid workOrderId, CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<PartUsageDto>>($"v1/maintenance/work-orders/{workOrderId}/parts", cancellationToken);

    /// <summary>Retrieves the chronological event history for a work order.</summary>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<MaintenanceEventDto>>> GetWorkOrderHistoryAsync(Guid workOrderId, CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<MaintenanceEventDto>>($"v1/maintenance/work-orders/{workOrderId}/history", cancellationToken);

    /// <summary>Retrieves fresh predictive maintenance insights for a work order (never cached).</summary>
    public Task<ApiResponseEnvelope<MaintenancePredictiveInsightsDto>> GetPredictiveInsightsAsync(Guid workOrderId, CancellationToken cancellationToken = default)
        => GetAsync<MaintenancePredictiveInsightsDto>($"v1/maintenance/work-orders/{workOrderId}/predictive", cancellationToken);

    /// <summary>Retrieves the chronological maintenance history for a single machine.</summary>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<MaintenanceEventDto>>> GetMachineHistoryAsync(
        Guid machineId, int pageNumber = 1, int pageSize = 25, CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<MaintenanceEventDto>>(
            $"v1/maintenance/machines/{machineId}/history?pageNumber={pageNumber}&pageSize={pageSize}", cancellationToken);

    /// <summary>Retrieves the preventive maintenance task list.</summary>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<PreventiveTaskDto>>> GetPreventiveTasksAsync(
        int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<PreventiveTaskDto>>(
            $"v1/maintenance/preventive?pageNumber={pageNumber}&pageSize={pageSize}", cancellationToken);

    /// <summary>Retrieves technician workload cards.</summary>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<TechnicianWorkloadDto>>> GetTechnicianWorkloadAsync(
        int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<TechnicianWorkloadDto>>(
            $"v1/maintenance/technicians/workload?pageNumber={pageNumber}&pageSize={pageSize}", cancellationToken);

    /// <summary>Retrieves the parts inventory list.</summary>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<PartInventoryDto>>> GetPartsInventoryAsync(
        int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<PartInventoryDto>>(
            $"v1/maintenance/parts?pageNumber={pageNumber}&pageSize={pageSize}", cancellationToken);

    /// <summary>Retrieves the maintenance calendar events within a date range (Volume MC1).</summary>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<MaintenanceCalendarEventDto>>> GetCalendarEventsAsync(
        DateOnly rangeStart, DateOnly rangeEnd, CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<MaintenanceCalendarEventDto>>(
            $"v1/maintenance/calendar?start={rangeStart:yyyy-MM-dd}&end={rangeEnd:yyyy-MM-dd}", cancellationToken);

    // ==== Writes ================================================================================

    /// <summary>Creates a new work order (CreateWorkOrderCommand).</summary>
    public Task<ApiResponseEnvelope<CreateWorkOrderResult>> CreateWorkOrderAsync(
        CreateWorkOrderRequest request, CancellationToken cancellationToken = default)
        => PostAsync<CreateWorkOrderRequest, CreateWorkOrderResult>("v1/maintenance/work-orders", request, cancellationToken);

    /// <summary>(Re)assigns a technician to an existing work order (AssignWorkOrderCommand).</summary>
    public Task<ApiResponseEnvelope<WorkOrderStatusResult>> AssignWorkOrderAsync(
        Guid workOrderId, AssignWorkOrderRequest request, CancellationToken cancellationToken = default)
        => PostAsync<AssignWorkOrderRequest, WorkOrderStatusResult>($"v1/maintenance/work-orders/{workOrderId}/assign", request, cancellationToken);

    /// <summary>Applies a Start/PutOnHold/Resume/Cancel lifecycle transition (UpdateWorkOrderStatusCommand).</summary>
    public Task<ApiResponseEnvelope<WorkOrderStatusResult>> UpdateWorkOrderStatusAsync(
        Guid workOrderId, UpdateWorkOrderStatusRequest request, CancellationToken cancellationToken = default)
        => PostAsync<UpdateWorkOrderStatusRequest, WorkOrderStatusResult>($"v1/maintenance/work-orders/{workOrderId}/status", request, cancellationToken);

    /// <summary>Completes an in-progress work order (CompleteWorkOrderCommand).</summary>
    public Task<ApiResponseEnvelope<WorkOrderStatusResult>> CompleteWorkOrderAsync(
        Guid workOrderId, CompleteWorkOrderRequest request, CancellationToken cancellationToken = default)
        => PostAsync<CompleteWorkOrderRequest, WorkOrderStatusResult>($"v1/maintenance/work-orders/{workOrderId}/complete", request, cancellationToken);

    /// <summary>Adjusts a part's on-hand stock quantity (AdjustPartStockCommand).</summary>
    public Task<ApiResponseEnvelope<PartStockResult>> AdjustPartStockAsync(
        Guid partId, AdjustPartStockRequest request, CancellationToken cancellationToken = default)
        => PostAsync<AdjustPartStockRequest, PartStockResult>($"v1/maintenance/parts/{partId}/adjust-stock", request, cancellationToken);
}
