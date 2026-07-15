// ============================================================================
// File: SustainabilityV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-15
// Description: V1 controller exposing Sustainability Commands and Queries from the
//              Application-layer Features/ CQRS tree over HTTP.
// ============================================================================
//
// Onboarding Notes:
// - Routes follow /api/v1/sustainability/metrics (matches the route already
//   coded into PlantSafety.UI's SustainabilityApiClient).
//
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Features.Sustainability.Contracts.Requests;
using PlantSafety.Api.Features.Sustainability.Contracts.Responses;
using PlantSafety.Application.Features.Sustainability.Commands.RecordMetric;
using PlantSafety.Application.Features.Sustainability.Models;
using PlantSafety.Application.Features.Sustainability.Queries.GetMetric;
using PlantSafety.Application.Features.Sustainability.Queries.ListMetrics;

namespace PlantSafety.Api.Features.Sustainability.Controllers.V1;

/// <summary>V1 controller for Sustainability command and query endpoints.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/sustainability/metrics")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Sustainability")]
public sealed class SustainabilityV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="SustainabilityV1Controller"/>.</summary>
    public SustainabilityV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Records a new sustainability metric observation.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<RecordSustainabilityMetricResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RecordAsync(
        [FromBody] RecordSustainabilityMetricRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RecordSustainabilityMetricCommand
        {
            CommandId = env.CommandId,
            CorrelationId = env.CorrelationId,
            CausationId = env.CausationId,
            RequestedAtUtc = env.RequestedAtUtc,
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId,
            ActorId = env.ActorId,
            SiteId = request.SiteId,
            MetricName = request.MetricName,
            Category = request.Category,
            Value = request.Value,
            Unit = request.Unit,
            Target = request.Target,
            PeriodEndUtc = request.PeriodEndUtc,
        };

        var metricId = await _sender.Send(command, cancellationToken);

        return Ok(ApiResponse<RecordSustainabilityMetricResponse>.Ok(
            new RecordSustainabilityMetricResponse(metricId), env.CorrelationId.ToString()));
    }

    /// <summary>Returns a single sustainability metric by identifier.</summary>
    [HttpGet("{metricId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SustainabilityMetricResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(Guid metricId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetSustainabilityMetricByIdQuery
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
            MetricId = metricId,
        };

        var metric = await _sender.Send(query, cancellationToken);

        if (metric is null)
            return NotFound(ApiResponse<SustainabilityMetricResponse>.Fail("Sustainability metric not found.", env.CorrelationId.ToString()));

        return Ok(ApiResponse<SustainabilityMetricResponse>.Ok(ToResponse(metric), env.CorrelationId.ToString()));
    }

    /// <summary>Returns sustainability metrics for the tenant within the requested reporting window.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<SustainabilityListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListAsync(
        [FromQuery] Guid? siteId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var effectiveFrom = from ?? DateTime.UtcNow.AddDays(-365);
        var effectiveTo = to ?? DateTime.UtcNow;

        var query = new ListSustainabilityMetricsQuery
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
            SiteId = siteId,
            From = effectiveFrom,
            To = effectiveTo,
        };

        var items = await _sender.Send(query, cancellationToken);

        var response = new SustainabilityListResponse(env.TenantId, siteId, DateTime.UtcNow, items.Select(ToResponse).ToList());

        return Ok(ApiResponse<SustainabilityListResponse>.Ok(response, env.CorrelationId.ToString()));
    }

    private static SustainabilityMetricResponse ToResponse(SustainabilityMetricDto dto) => new(
        dto.MetricId,
        dto.TenantId,
        dto.SiteId,
        dto.MetricName,
        dto.Category,
        dto.Value,
        dto.Unit,
        dto.Target,
        dto.PeriodEndUtc,
        dto.Status);
}
