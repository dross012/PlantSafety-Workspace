// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Api
//  File:           ProductionTrackingV1Controller.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    V1 controller for the Operational.ProductionTracking module. New-build core for
//                  the ProductionTracking UI (v1/production-tracking/*) -- see
//                  docs/Phase5/ui-gaps-scoping.md "ProductionTracking" section (largest, riskiest
//                  scope of the 9 UI gaps; previously had no Application/API/Domain layer at all).
//                  Route matches PlantSafety.UI.Modules.ProductionTracking.ApiClients
//                  .ProductionTrackingApiClient's pre-existing route expectations exactly, so no UI
//                  route changes were needed -- only the DTO shapes were aligned. Routes all
//                  reads/writes through MediatR ISender to the Application layer; no domain logic or
//                  data access here. The 13 GET endpoints reuse the real Phase 5
//                  Observability.Telemetry/Analytics.Dashboard.Kpi infrastructure (see
//                  ProductionTrackingKpiQueryHandlers.cs) rather than reimplementing KPI/anomaly
//                  math. The 5 POST endpoints are not currently called by the UI (which is 100%
//                  read-only per its Razor pages) but exist so the module's read-model tables can
//                  genuinely be populated. Additive-only evolution.
// ============================================================================
using PlantSafety.Api.Context;
using PlantSafety.Api.Common;
using PlantSafety.Api.Operational.ProductionTracking.Authorization;
using PlantSafety.Api.Operational.ProductionTracking.Contracts.Requests;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Contracts.Dtos;
using PlantSafety.Application.Operational.ProductionTracking.Commands;
using PlantSafety.Application.Operational.ProductionTracking.Models;
using PlantSafety.Application.Operational.ProductionTracking.Queries;

namespace PlantSafety.Api.Operational.ProductionTracking.Controllers.V1;

/// <summary>V1 controller for the Operational.ProductionTracking module.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/production-tracking")]
[Authorize(Policy = ProductionTrackingAuthorizationPolicies.Read)]
[Tags("ProductionTracking")]
public sealed class ProductionTrackingV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="ProductionTrackingV1Controller"/>.</summary>
    public ProductionTrackingV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    // ==== Reads ================================================================================

    /// <summary>Retrieves the Throughput Summary card.</summary>
    [HttpGet("throughput")]
    [ProducesResponseType(typeof(ApiResponse<ThroughputSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetThroughputAsync(CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new GetThroughputSummaryQuery(env.TenantId), cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves the Bottleneck Summary card.</summary>
    [HttpGet("bottlenecks")]
    [ProducesResponseType(typeof(ApiResponse<BottleneckSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBottlenecksAsync(CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new GetBottleneckSummaryQuery(env.TenantId), cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves a paginated page of active production runs.</summary>
    [HttpGet("runs")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponseDto<ProductionRunRowDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveRunsAsync(
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 25, [FromQuery] bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new ListActiveProductionRunsQuery(env.TenantId, pageNumber, pageSize), cancellationToken);
        return ToPaginatedActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves the Run Summary panel for a single run.</summary>
    [HttpGet("runs/{runId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<RunSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRunSummaryAsync(Guid runId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new GetRunSummaryQuery(env.TenantId, runId), cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves the Metadata panel for a single run.</summary>
    [HttpGet("runs/{runId:guid}/metadata")]
    [ProducesResponseType(typeof(ApiResponse<RunMetadataDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRunMetadataAsync(Guid runId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new GetRunMetadataQuery(env.TenantId, runId), cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves the chronological timeline for a single run.</summary>
    [HttpGet("runs/{runId:guid}/timeline")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponseDto<TimelineEventDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRunTimelineAsync(Guid runId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new GetRunTimelineQuery(env.TenantId, runId, pageNumber, pageSize), cancellationToken);
        return ToPaginatedActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves the batches produced by a single run.</summary>
    [HttpGet("runs/{runId:guid}/batches")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponseDto<BatchRowDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRunBatchesAsync(Guid runId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new GetRunBatchesQuery(env.TenantId, runId, pageNumber, pageSize), cancellationToken);
        return ToPaginatedActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves the KPI tiles for a single run.</summary>
    [HttpGet("runs/{runId:guid}/kpis")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponseDto<KpiTileDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRunKpisAsync(Guid runId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new GetRunKpisQuery(env.TenantId, runId, pageNumber, pageSize), cancellationToken);
        return ToPaginatedActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves the event log for a single run.</summary>
    [HttpGet("runs/{runId:guid}/events")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponseDto<EventRowDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRunEventsAsync(Guid runId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new GetRunEventsQuery(env.TenantId, runId, pageNumber, pageSize), cancellationToken);
        return ToPaginatedActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves a paginated page of batches across all runs.</summary>
    [HttpGet("batches")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponseDto<BatchRowDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBatchesAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new ListProductionBatchesQuery(env.TenantId, pageNumber, pageSize), cancellationToken);
        return ToPaginatedActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves plant-wide KPI tiles for the KPI Dashboard page.</summary>
    [HttpGet("kpis")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponseDto<KpiTileDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetKpisAsync(CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new GetPlantKpisQuery(env.TenantId, 1, 50), cancellationToken);
        return ToPaginatedActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves the plant-wide chronological production timeline.</summary>
    [HttpGet("timeline")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponseDto<TimelineEventDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTimelineAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new GetProductionTimelineQuery(env.TenantId, pageNumber, pageSize), cancellationToken);
        return ToPaginatedActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves the searchable/filterable production event history log.</summary>
    [HttpGet("events")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponseDto<EventRowDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEventHistoryAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new GetEventHistoryQuery(env.TenantId, pageNumber, pageSize), cancellationToken);
        return ToPaginatedActionResult(result, env.CorrelationId);
    }

    // ==== Writes ================================================================================
    // Not currently called by the UI (100% read-only per its Razor pages); exist so the module's
    // read-model tables can genuinely be populated ahead of a future manual-entry flow or
    // ingestion-side integration.

    /// <summary>Registers a new ProductionLine master-data record.</summary>
    [HttpPost("lines")]
    [Authorize(Policy = ProductionTrackingAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CreateProductionLineResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CreateProductionLineResult>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProductionLineAsync([FromBody] CreateProductionLineRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateProductionLineCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            LineCode: request.LineCode,
            LineName: request.LineName,
            RequestedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Starts a new ProductionRun.</summary>
    [HttpPost("runs")]
    [Authorize(Policy = ProductionTrackingAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<StartProductionRunResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<StartProductionRunResult>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StartProductionRunAsync([FromBody] StartProductionRunRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new StartProductionRunCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            RunCode: request.RunCode,
            LineId: request.LineId,
            LineName: request.LineName,
            MachineId: request.MachineId,
            OperatorName: request.OperatorName ?? string.Empty,
            IdealCycleTimeSeconds: request.IdealCycleTimeSeconds,
            RequestedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records a timeline event (optionally a downtime interval) against an in-progress ProductionRun.</summary>
    [HttpPost("runs/{runId:guid}/events")]
    [Authorize(Policy = ProductionTrackingAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<ProductionRunEventResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProductionRunEventResult>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RecordProductionRunEventAsync(Guid runId, [FromBody] RecordProductionRunEventRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RecordProductionRunEventCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            RunId: runId,
            RunEventType: request.RunEventType,
            Description: request.Description ?? string.Empty,
            IsDowntime: request.IsDowntime,
            DowntimeReasonCode: request.DowntimeReasonCode,
            DowntimeMinutes: request.DowntimeMinutes,
            GoodUnitsDelta: request.GoodUnitsDelta,
            ScrapUnitsDelta: request.ScrapUnitsDelta,
            RequestedByUserId: env.ActorId)
        {
            ETag = request.ETag
        };

        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Adds a Batch to an in-progress ProductionRun.</summary>
    [HttpPost("runs/{runId:guid}/batches")]
    [Authorize(Policy = ProductionTrackingAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<AddProductionBatchResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AddProductionBatchResult>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddProductionBatchAsync(Guid runId, [FromBody] AddProductionBatchRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new AddProductionBatchCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            RunId: runId,
            BatchCode: request.BatchCode,
            Status: request.Status,
            RequestedByUserId: env.ActorId)
        {
            ETag = request.ETag
        };

        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Ends an in-progress ProductionRun (Completed or Failed).</summary>
    [HttpPost("runs/{runId:guid}/complete")]
    [Authorize(Policy = ProductionTrackingAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<ProductionRunStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProductionRunStatusResult>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CompleteProductionRunAsync(Guid runId, [FromBody] CompleteProductionRunRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CompleteProductionRunCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            RunId: runId,
            Succeeded: request.Succeeded,
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
