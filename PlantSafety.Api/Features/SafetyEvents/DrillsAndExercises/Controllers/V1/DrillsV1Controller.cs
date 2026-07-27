// ============================================================================
// File: DrillsV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: V1 MVC controller exposing the Create/List/GetById endpoints for the
//              Features/SafetyEvents Drills and Exercises module.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/features/safety-events/drills/...
// - CreateDrillCommand carries the full correlation-lineage envelope (§2.1 of the Enterprise
//   Architectural Rule Handbook). RequestEnvelopeFactory builds those fields from the current
//   request's IOperationContext/ICurrentUserService.
//
// Rationale:
// - Routes all writes/reads through MediatR ISender to the Application layer. No domain
//   logic or data access here.
//
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Features.SafetyEvents.DrillsAndExercises.Contracts.Requests;
using PlantSafety.Api.Features.SafetyEvents.DrillsAndExercises.Contracts.Responses;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Features.SafetyEvents.Commands.DrillsAndExercises.Create;
using PlantSafety.Application.Features.SafetyEvents.Models.DrillsAndExercises;
using PlantSafety.Application.Features.SafetyEvents.Queries.DrillsAndExercises.Get;
using PlantSafety.Application.Features.SafetyEvents.Queries.DrillsAndExercises.List;

namespace PlantSafety.Api.Features.SafetyEvents.DrillsAndExercises.Controllers.V1;

/// <summary>V1 controller for Drills and Exercises command and read endpoints.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/features/safety-events/drills")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Drills")]
public sealed class DrillsV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="DrillsV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds Command/Query envelope fields from the current request.</param>
    public DrillsV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Schedules a Safety Drill or Exercise.</summary>
    /// <param name="request">Request body describing the drill to schedule.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the new drill identifier.</returns>
    /// <response code="201">Drill scheduled successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CreateDrillResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateDrillRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateDrillCommand
        {
            CommandId = env.CommandId,
            CorrelationId = env.CorrelationId,
            CausationId = env.CausationId,
            RequestedAtUtc = env.RequestedAtUtc,
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId,
            ActorId = env.ActorId,
            SiteId = request.SiteId,
            Title = request.Title,
            Description = request.Description,
            DrillType = request.DrillType,
            ScheduledAtUtc = request.ScheduledAtUtc,
            CoordinatorId = request.CoordinatorId,
        };

        var drillId = await _sender.Send(command, cancellationToken);

        var response = new CreateDrillResponse(drillId);

        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<CreateDrillResponse>.Ok(response, env.CorrelationId.ToString()));
    }

    /// <summary>Returns a paginated, filterable list of Safety Drills / Exercises for the caller's tenant.</summary>
    /// <param name="request">Filter and paging parameters bound from the query string.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>The canonical <see cref="PaginatedResult{T}"/> envelope wrapping drills.</returns>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(PaginatedResult<DrillDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListAsync(
        [FromQuery] ListDrillsRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new ListDrillsQuery
        {
            TraceId = env.TraceId,
            SpanId = env.SpanId,
            ApiVersionToken = env.ApiVersionToken,
            RequestingUserId = env.RequestingUserId,
            RequestingUserRole = env.RequestingUserRole,
            TenantId = env.TenantId,
            CorrelationId = env.CorrelationId,
            CachePreference = env.CachePreference,
            SuppressUnauthorized = env.SuppressUnauthorized,
            StrictMode = env.StrictMode,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            OrderBy = request.OrderBy,
            SortDirection = request.SortDirection,
            SiteId = request.SiteId,
            Status = request.Status,
            DrillType = request.DrillType
        };

        var result = await _sender.Send(query, cancellationToken);

        return Ok(result);
    }

    /// <summary>Returns the full projection for a given Safety Drill / Exercise.</summary>
    /// <param name="drillId">Drill to retrieve.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>The <see cref="DrillDto"/> when found.</returns>
    /// <response code="200">Drill found.</response>
    /// <response code="404">Drill not found.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("{drillId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(DrillDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        [FromRoute] Guid drillId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetDrillByIdQuery
        {
            TraceId = env.TraceId,
            SpanId = env.SpanId,
            ApiVersionToken = env.ApiVersionToken,
            RequestingUserId = env.RequestingUserId,
            RequestingUserRole = env.RequestingUserRole,
            TenantId = env.TenantId,
            CorrelationId = env.CorrelationId,
            CachePreference = env.CachePreference,
            SuppressUnauthorized = env.SuppressUnauthorized,
            StrictMode = env.StrictMode,
            DrillId = drillId
        };

        var drill = await _sender.Send(query, cancellationToken);

        if (drill is null)
            return NotFound();

        return Ok(drill);
    }
}
