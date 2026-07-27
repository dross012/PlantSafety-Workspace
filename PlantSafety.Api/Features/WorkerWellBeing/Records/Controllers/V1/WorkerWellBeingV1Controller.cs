// ============================================================================
// File: WorkerWellBeingV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-15
// Description: V1 controller exposing WorkerWellBeing/Records Queries from the
//              Application-layer Features/ CQRS tree over HTTP.
// ============================================================================
//
// Onboarding Notes:
// - Routes follow /api/v1/human-factors/worker-wellbeing/... (matches the route the
//   PlantSafety.UI WorkerWellBeingApiClient already targets, so no UI route change
//   was required — only the ApiClient's degraded-state comments were updated).
// - Command/Query envelope fields are populated via RequestEnvelopeFactory,
//   which reads IOperationContext and ICurrentUserService for the current request.
//
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Features.WorkerWellBeing.Records.Contracts.Requests;
using PlantSafety.Api.Features.WorkerWellBeing.Records.Contracts.Responses;
using PlantSafety.Application.Features.WorkerWellBeing.Models.Records;
using PlantSafety.Application.Features.WorkerWellBeing.Queries.Records.GetRecord;
using PlantSafety.Application.Features.WorkerWellBeing.Queries.Records.ListRecords;

namespace PlantSafety.Api.Features.WorkerWellBeing.Records.Controllers.V1;

/// <summary>V1 controller for WorkerWellBeing/Records query endpoints.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/human-factors/worker-wellbeing")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("WorkerWellBeing")]
public sealed class WorkerWellBeingV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="WorkerWellBeingV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds Command/Query envelope fields from the current request context.</param>
    public WorkerWellBeingV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Returns a single Worker WellBeing record by identifier.</summary>
    /// <param name="workerWellBeingId">Record identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the record, or 404 when not found.</returns>
    /// <response code="200">Record returned successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpGet("{workerWellBeingId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<WorkerWellBeingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(
        Guid workerWellBeingId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetWorkerWellBeingByIdQuery
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
            WorkerWellBeingId = workerWellBeingId,
        };

        var record = await _sender.Send(query, cancellationToken);

        if (record is null)
            return NotFound(ApiResponse<WorkerWellBeingResponse>.Fail("Worker WellBeing record not found.", env.CorrelationId.ToString()));

        return Ok(ApiResponse<WorkerWellBeingResponse>.Ok(ToResponse(record), env.CorrelationId.ToString()));
    }

    /// <summary>Returns a paginated list of Worker WellBeing records for the tenant.</summary>
    /// <param name="request">Query parameters bound from the query string.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the paginated record list.</returns>
    /// <response code="200">Records returned successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<WorkerWellBeingListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListAsync(
        [FromQuery] ListWorkerWellBeingRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new ListWorkerWellBeingQuery
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
            WorkerId = request.WorkerId,
            WellBeingRiskLevel = request.WellBeingRiskLevel,
            Status = request.Status,
        };

        var result = await _sender.Send(query, cancellationToken);

        var response = new WorkerWellBeingListResponse(
            result.Items.Select(ToResponse).ToList(),
            result.TotalCount,
            result.PageNumber,
            result.PageSize,
            result.TotalPages,
            result.HasNextPage,
            result.HasPreviousPage);

        return Ok(ApiResponse<WorkerWellBeingListResponse>.Ok(response, env.CorrelationId.ToString()));
    }

    private static WorkerWellBeingResponse ToResponse(WorkerWellBeingDto dto) => new(
        dto.WorkerWellBeingId,
        dto.TenantId,
        dto.WorkerId,
        dto.WellBeingRiskLevel,
        dto.Status,
        dto.RecordedAtUtc);
}
