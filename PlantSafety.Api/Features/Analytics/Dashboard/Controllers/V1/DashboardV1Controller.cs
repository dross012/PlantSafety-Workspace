// ============================================================================
// File: DashboardV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: V1 MVC controller exposing the Application-layer
//              Features/Analytics/Dashboard Query over HTTP.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/features/analytics/dashboard/...
// - Envelope fields are populated via the shared RequestEnvelopeFactory.
//
// Rationale:
// - Routes all reads through MediatR ISender to the Application layer. No domain
//   logic or data access here.
//
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Features.Analytics.Dashboard.Contracts.Requests;
using PlantSafety.Application.Features.Analytics.Models.Dashboard;
using PlantSafety.Application.Features.Analytics.Queries.Dashboard;

namespace PlantSafety.Api.Features.Analytics.Dashboard.Controllers.V1;

/// <summary>V1 controller for the Safety Dashboard KPI snapshot query.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/features/analytics/dashboard")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Dashboard")]
public sealed class DashboardV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="DashboardV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public DashboardV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Returns the Safety Dashboard KPI snapshot for the caller's tenant.</summary>
    /// <param name="request">Filters bound from the query string.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the <see cref="SafetyDashboardDto"/>.</returns>
    /// <response code="200">Dashboard snapshot returned successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<SafetyDashboardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAsync(
        [FromQuery] GetSafetyDashboardRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetSafetyDashboardQuery
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
            SiteId = request.SiteId,
            WindowFromUtc = request.WindowFromUtc,
            WindowToUtc = request.WindowToUtc
        };

        var dashboard = await _sender.Send(query, cancellationToken);

        return Ok(ApiResponse<SafetyDashboardDto>.Ok(dashboard, env.CorrelationId.ToString()));
    }
}
