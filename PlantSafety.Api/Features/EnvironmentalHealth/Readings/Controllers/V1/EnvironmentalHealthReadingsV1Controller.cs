// ============================================================================
// File: EnvironmentalHealthReadingsV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-15
// Description: V1 controller exposing EnvironmentalHealth/Readings Queries from the
//              Application-layer Features/ CQRS tree over HTTP. This is the first
//              dedicated Environmental Health endpoint — previously the UI borrowed
//              GET v1/compliance/policy/environmental as a stand-in.
// ============================================================================
//
// Onboarding Notes:
// - Routes follow /api/v1/features/environmental-health/readings/...
// - Command/Query envelope fields are populated via RequestEnvelopeFactory,
//   which reads IOperationContext and ICurrentUserService for the current request.
//
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Features.EnvironmentalHealth.Readings.Contracts.Requests;
using PlantSafety.Api.Features.EnvironmentalHealth.Readings.Contracts.Responses;
using PlantSafety.Application.Features.EnvironmentalHealth.Models.Readings;
using PlantSafety.Application.Features.EnvironmentalHealth.Queries.Readings.GetReading;
using PlantSafety.Application.Features.EnvironmentalHealth.Queries.Readings.ListReadings;

namespace PlantSafety.Api.Features.EnvironmentalHealth.Readings.Controllers.V1;

/// <summary>V1 controller for EnvironmentalHealth/Readings query endpoints.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/features/environmental-health/readings")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("EnvironmentalHealth")]
public sealed class EnvironmentalHealthReadingsV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="EnvironmentalHealthReadingsV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds Command/Query envelope fields from the current request context.</param>
    public EnvironmentalHealthReadingsV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Returns a single Environmental Health reading by identifier.</summary>
    /// <param name="environmentalHealthId">Reading identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the reading, or 404 when not found.</returns>
    /// <response code="200">Reading returned successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Reading not found.</response>
    [HttpGet("{environmentalHealthId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<EnvironmentalHealthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(
        Guid environmentalHealthId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetEnvironmentalHealthByIdQuery
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
            EnvironmentalHealthId = environmentalHealthId,
        };

        var reading = await _sender.Send(query, cancellationToken);

        if (reading is null)
            return NotFound(ApiResponse<EnvironmentalHealthResponse>.Fail("Environmental Health reading not found.", env.CorrelationId.ToString()));

        return Ok(ApiResponse<EnvironmentalHealthResponse>.Ok(ToResponse(reading), env.CorrelationId.ToString()));
    }

    /// <summary>Returns a paginated list of Environmental Health readings for the tenant.</summary>
    /// <param name="request">Query parameters bound from the query string.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the paginated reading list.</returns>
    /// <response code="200">Readings returned successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<EnvironmentalHealthListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListAsync(
        [FromQuery] ListEnvironmentalHealthRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new ListEnvironmentalHealthQuery
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
            MetricType = request.MetricType,
            Status = request.Status,
            From = request.From,
            To = request.To,
        };

        var result = await _sender.Send(query, cancellationToken);

        var response = new EnvironmentalHealthListResponse(
            result.Items.Select(ToResponse).ToList(),
            result.TotalCount,
            result.PageNumber,
            result.PageSize,
            result.TotalPages,
            result.HasNextPage,
            result.HasPreviousPage);

        return Ok(ApiResponse<EnvironmentalHealthListResponse>.Ok(response, env.CorrelationId.ToString()));
    }

    private static EnvironmentalHealthResponse ToResponse(EnvironmentalHealthDto dto) => new(
        dto.EnvironmentalHealthId,
        dto.TenantId,
        dto.SiteId,
        dto.MetricType,
        dto.MeasurementValue,
        dto.Unit,
        dto.Threshold,
        dto.Status,
        dto.RecordedAtUtc);
}
