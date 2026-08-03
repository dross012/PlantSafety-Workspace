// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Api
//  File:           WorkOrdersV1Controller.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    V1 controller for the Operational.WorkOrders module. New-build core for the
//                  Maintenance UI (v1/maintenance/*) -- see docs/Phase5/ui-gaps-scoping.md
//                  "Maintenance" section. Deliberately distinct route prefix
//                  (api/v{version}/maintenance) and namespace (Operational.WorkOrders) from the
//                  pre-existing, narrower api/v{version}/operational/preventive-maintenance and
//                  api/v{version}/operational/predictive-maintenance controllers: WorkOrders is the
//                  operational execution layer (work orders/parts/technicians/calendar); Preventive/
//                  Predictive are the planning/scoring layers that feed it. Two sub-resources
//                  (GetByIdPredictive, GetPreventiveTasks) deliberately delegate into those sibling
//                  modules' real query surface via MediatR rather than duplicating logic here --
//                  see WorkOrderReuseQueryHandlers.cs. Routes all reads/writes through MediatR
//                  ISender to the Application layer; no domain logic or data access here.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Api.Context;
using PlantSafety.Api.Common;
using PlantSafety.Api.Operational.WorkOrders.Authorization;
using PlantSafety.Api.Operational.WorkOrders.Contracts.Requests;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Contracts.Dtos;
using PlantSafety.Application.Operational.WorkOrders.Commands;
using PlantSafety.Application.Operational.WorkOrders.Models;
using PlantSafety.Application.Operational.WorkOrders.Queries;

namespace PlantSafety.Api.Operational.WorkOrders.Controllers.V1;

/// <summary>V1 controller for the Operational.WorkOrders module (Maintenance UI core).</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/maintenance")]
[Authorize(Policy = WorkOrdersAuthorizationPolicies.Read)]
[Tags("WorkOrders")]
public sealed class WorkOrdersV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="WorkOrdersV1Controller"/>.</summary>
    public WorkOrdersV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    // ==== Reads ================================================================================

    /// <summary>Retrieves the Work Order Summary card counts.</summary>
    [HttpGet("work-orders/summary")]
    [ProducesResponseType(typeof(ApiResponse<WorkOrderSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWorkOrderSummaryAsync(CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new GetWorkOrderSummaryQuery(env.TenantId), cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves the Machine Status Summary card counts.</summary>
    [HttpGet("machines/status-summary")]
    [ProducesResponseType(typeof(ApiResponse<MachineStatusSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMachineStatusSummaryAsync(CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new GetMachineStatusSummaryQuery(env.TenantId), cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves a paginated page of active work orders.</summary>
    [HttpGet("work-orders")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponseDto<WorkOrderRowDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveWorkOrdersAsync(
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 25, [FromQuery] string? status = null, [FromQuery] string? technician = null,
        CancellationToken cancellationToken = default)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new ListWorkOrdersQuery(env.TenantId, pageNumber, pageSize, status, technician), cancellationToken);
        return ToPaginatedActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves the full detail for a single work order.</summary>
    [HttpGet("work-orders/{workOrderId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<WorkOrderItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWorkOrderAsync(Guid workOrderId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new GetWorkOrderByIdQuery(env.TenantId, workOrderId), cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves the ordered task steps for a work order.</summary>
    [HttpGet("work-orders/{workOrderId:guid}/tasks")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponseDto<TaskStepDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTasksAsync(Guid workOrderId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new GetWorkOrderTasksQuery(env.TenantId, workOrderId, pageNumber, pageSize), cancellationToken);
        return ToPaginatedActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves the parts consumed by a work order.</summary>
    [HttpGet("work-orders/{workOrderId:guid}/parts")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponseDto<PartUsageDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPartsAsync(Guid workOrderId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new GetWorkOrderPartsUsageQuery(env.TenantId, workOrderId, pageNumber, pageSize), cancellationToken);
        return ToPaginatedActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves the chronological event history for a work order.</summary>
    [HttpGet("work-orders/{workOrderId:guid}/history")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponseDto<MaintenanceEventDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWorkOrderHistoryAsync(Guid workOrderId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new GetWorkOrderHistoryQuery(env.TenantId, workOrderId, pageNumber, pageSize), cancellationToken);
        return ToPaginatedActionResult(result, env.CorrelationId);
    }

    /// <summary>
    /// Reuse point: retrieves fresh predictive maintenance insights for a work order's target
    /// machine, delegating to the real Operational.PredictiveMaintenance module. Never cached.
    /// </summary>
    [HttpGet("work-orders/{workOrderId:guid}/predictive")]
    [ProducesResponseType(typeof(ApiResponse<MaintenancePredictiveInsightsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPredictiveInsightsAsync(Guid workOrderId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope(cachePreference: CachePreference.None);
        var result = await _sender.Send(new GetWorkOrderPredictiveInsightsQuery(env.TenantId, workOrderId), cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves the chronological maintenance history for a single machine.</summary>
    [HttpGet("machines/{machineId:guid}/history")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponseDto<MaintenanceEventDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMachineHistoryAsync(Guid machineId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new GetMachineHistoryQuery(env.TenantId, machineId, pageNumber, pageSize), cancellationToken);
        return ToPaginatedActionResult(result, env.CorrelationId);
    }

    /// <summary>
    /// Reuse point: retrieves the preventive maintenance task list, delegating to the real
    /// Operational.PreventiveMaintenance module rather than a second, duplicated concept.
    /// </summary>
    [HttpGet("preventive")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponseDto<PreventiveTaskDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPreventiveTasksAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new GetWorkOrderPreventiveTasksQuery(env.TenantId, pageNumber, pageSize), cancellationToken);
        return ToPaginatedActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves technician workload cards.</summary>
    [HttpGet("technicians/workload")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponseDto<TechnicianWorkloadDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTechnicianWorkloadAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new GetTechnicianWorkloadQuery(env.TenantId, pageNumber, pageSize), cancellationToken);
        return ToPaginatedActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves the parts inventory list.</summary>
    [HttpGet("parts")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponseDto<PartInventoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPartsInventoryAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new ListPartsInventoryQuery(env.TenantId, pageNumber, pageSize), cancellationToken);
        return ToPaginatedActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves the maintenance calendar events within a date range.</summary>
    [HttpGet("calendar")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponseDto<MaintenanceCalendarEventDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCalendarEventsAsync([FromQuery] DateOnly start, [FromQuery] DateOnly end, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new GetMaintenanceCalendarQuery(env.TenantId, start, end), cancellationToken);
        return ToPaginatedActionResult(result, env.CorrelationId);
    }

    // ==== Writes ================================================================================

    /// <summary>Creates a new work order.</summary>
    [HttpPost("work-orders")]
    [Authorize(Policy = WorkOrdersAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CreateWorkOrderResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CreateWorkOrderResult>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateWorkOrderAsync([FromBody] CreateWorkOrderRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateWorkOrderCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            WorkOrderCode: request.WorkOrderCode,
            MachineId: request.MachineId,
            MachineCode: request.MachineCode,
            Priority: request.Priority,
            Description: request.Description,
            DueAtUtc: request.DueAtUtc,
            RequestedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>(Re)assigns a technician to an existing work order.</summary>
    [HttpPost("work-orders/{workOrderId:guid}/assign")]
    [Authorize(Policy = WorkOrdersAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<WorkOrderStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<WorkOrderStatusResult>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssignWorkOrderAsync(Guid workOrderId, [FromBody] AssignWorkOrderRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new AssignWorkOrderCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            WorkOrderId: workOrderId,
            TechnicianId: request.TechnicianId,
            TechnicianName: request.TechnicianName,
            RequestedByUserId: env.ActorId)
        {
            ETag = request.ETag
        };

        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Applies a Start/PutOnHold/Resume/Cancel lifecycle transition to an existing work order.</summary>
    [HttpPost("work-orders/{workOrderId:guid}/status")]
    [Authorize(Policy = WorkOrdersAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<WorkOrderStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<WorkOrderStatusResult>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateWorkOrderStatusAsync(Guid workOrderId, [FromBody] UpdateWorkOrderStatusRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new UpdateWorkOrderStatusCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            WorkOrderId: workOrderId,
            TargetStatus: request.TargetStatus,
            Reason: request.Reason,
            RequestedByUserId: env.ActorId)
        {
            ETag = request.ETag
        };

        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Completes an in-progress work order.</summary>
    [HttpPost("work-orders/{workOrderId:guid}/complete")]
    [Authorize(Policy = WorkOrdersAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<WorkOrderStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<WorkOrderStatusResult>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CompleteWorkOrderAsync(Guid workOrderId, [FromBody] CompleteWorkOrderRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CompleteWorkOrderCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            WorkOrderId: workOrderId,
            CompletionNotes: request.CompletionNotes,
            RequestedByUserId: env.ActorId)
        {
            ETag = request.ETag
        };

        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Adjusts a part's on-hand stock quantity (restock or consumption).</summary>
    [HttpPost("parts/{partId:guid}/adjust-stock")]
    [Authorize(Policy = WorkOrdersAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PartStockResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PartStockResult>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AdjustPartStockAsync(Guid partId, [FromBody] AdjustPartStockRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new AdjustPartStockCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PartId: partId,
            DeltaQuantity: request.DeltaQuantity,
            Reason: request.Reason,
            RequestedByUserId: env.ActorId)
        {
            ETag = request.ETag
        };

        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ==== Helpers ===============================================================================

    /// <summary>Maps a paginated Result onto the canonical ApiResponse&lt;PaginatedResponseDto&lt;T&gt;&gt; shape.</summary>
    private IActionResult ToPaginatedActionResult<T>(PlantSafety.Application.Common.Results.Result<PlantSafety.Application.Common.Results.PaginatedResult<T>> result, Guid correlationId)
    {
        if (result.IsFailure)
        {
            return this.ToActionResult(result, correlationId);
        }

        var dto = PaginatedResponseDto<T>.FromResult(result.Value);
        return Ok(ApiResponse<PaginatedResponseDto<T>>.Ok(dto, correlationId.ToString()));
    }
}
