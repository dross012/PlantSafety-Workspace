// ============================================================================
// File: SchedulingJobsV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (Claude Code modular-creation-agent)
// Created: 2026-08-03
// Description: V1 controller for the Scheduling.Rollup module -- backs the Scheduling UI's
//              Dashboard/Job Detail/Calendar pages (v1/scheduling/jobs*, v1/scheduling/calendar,
//              v1/scheduling/resources/summary). Deliberately a *separate* controller class
//              sharing the same "api/v{version}/scheduling" route prefix as the pre-existing
//              SchedulingV1Controller (PMScheduling/WorkOrderPriority/EmergencyDrillPlanning/
//              HazardIsolationSequencing/ShiftFatigue) rather than editing that large existing
//              file, avoiding merge friction. Per docs/Phase5/ui-gaps-scoping.md's Scheduling
//              section, "Job" is NOT a new domain aggregate here -- every action below dispatches
//              to the Scheduling.Rollup Application-layer queries, which themselves fan out to the
//              real Operational.WorkOrders and Scheduling.PMScheduling query surfaces via ISender.
//              Routes all reads through MediatR ISender to the Application layer. No domain logic
//              or data access here.
// ============================================================================
using PlantSafety.Api.Context;
using PlantSafety.Api.Common;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Contracts.Dtos;
using PlantSafety.Application.Scheduling.Rollup.Models;
using PlantSafety.Application.Scheduling.Rollup.Queries;

namespace PlantSafety.Api.Scheduling.Controllers.V1;

/// <summary>V1 controller for the Scheduling.Rollup module's "Job"/calendar/resource-summary queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/scheduling")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Scheduling")]
public sealed class SchedulingJobsV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="SchedulingJobsV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public SchedulingJobsV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Retrieves the Job Summary card counts, rolled up across WorkOrders + PMScheduling.</summary>
    [HttpGet("jobs/summary")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<JobSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetJobSummaryAsync(CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new GetJobSummaryQuery(env.TenantId), cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves the Resource Summary card counts, rolled up from WorkOrders' technician workload.</summary>
    [HttpGet("resources/summary")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<ResourceSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetResourceSummaryAsync(CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new GetResourceSummaryQuery(env.TenantId), cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves a paginated page of upcoming jobs for the dashboard table, merged from WorkOrders + PMScheduling.</summary>
    [HttpGet("jobs")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponseDto<UpcomingJobDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUpcomingJobsAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new ListUpcomingJobsQuery(env.TenantId, pageNumber, pageSize, status), cancellationToken);
        return ToPaginatedActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves the full detail for a single job (a WorkOrder, or failing that, a PMScheduling record).</summary>
    [HttpGet("jobs/{jobId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<JobDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetJobDetailAsync(Guid jobId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new GetJobDetailQuery(env.TenantId, jobId), cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves the ordered timeline steps for a job.</summary>
    [HttpGet("jobs/{jobId:guid}/timeline")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponseDto<TimelineStepDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetJobTimelineAsync(Guid jobId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new GetJobTimelineQuery(env.TenantId, jobId), cancellationToken);
        return ToPaginatedActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves the scheduling constraints evaluated for a job.</summary>
    [HttpGet("jobs/{jobId:guid}/constraints")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponseDto<ConstraintDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetJobConstraintsAsync(Guid jobId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new GetJobConstraintsQuery(env.TenantId, jobId), cancellationToken);
        return ToPaginatedActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves fresh predictive scheduling insights for a job (never cached).</summary>
    [HttpGet("jobs/{jobId:guid}/predictive")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PredictiveInsightsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetJobPredictiveInsightsAsync(Guid jobId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope(cachePreference: CachePreference.None);
        var result = await _sender.Send(new GetJobPredictiveInsightsQuery(env.TenantId, jobId), cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves the calendar week grid starting at <paramref name="weekStart"/>, merged from WorkOrders + PMScheduling.</summary>
    [HttpGet("calendar")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<CalendarWeekDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCalendarWeekAsync([FromQuery] DateOnly weekStart, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new GetSchedulingCalendarWeekQuery(env.TenantId, weekStart), cancellationToken);
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
