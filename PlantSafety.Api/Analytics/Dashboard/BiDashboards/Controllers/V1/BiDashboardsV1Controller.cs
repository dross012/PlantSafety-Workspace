// ============================================================================
// File: BiDashboardsV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (API Contract Validation Agent)
// Created: 2026-08-02
// Last Updated: 2026-08-02
// Description: V1 controller exposing GET endpoints for the six Phase 5 §4.3 BI Dashboards
//              read-model queries (GetSchedulingDashboardQuery/GetProductionDashboardQuery/
//              GetMaintenanceDashboardQuery/GetRiskDashboardQuery/
//              GetContinuousImprovementDashboardQuery/GetAuditDashboardQuery). Mirrors
//              PlantSafety.Api.Analytics.Dashboard.Executive.Controllers.V1.ExecutiveV1Controller
//              and PlantSafety.Api.Analytics.Dashboard.KPISnapshots.Controllers.V1.KPISnapshotsV1Controller.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/analytics/dashboard/{scheduling|production|maintenance|risk|continuousimprovement|audit}
//   exactly as consumed by PlantSafety.UI.Modules.BiDashboards.ApiClients.BiDashboardsApiClient.
// - Route note: this controller's routes do not collide with the pre-existing read-only
//   Dashboard sub-feature controllers (Executive, KPISnapshots, ReadModel, TrendAnalysis), which
//   are routed under their own type-specific segments, nor with the root aggregate
//   DashboardV1Controller (routed at the bare "analytics/dashboard" base with no sub-segment).
//
// Rationale:
// - Routes all reads through MediatR ISender to the Application layer. No domain logic or data
//   access here.
//
// Future Extension Seams:
// - Add [OutputCache(PolicyName = ...)] attributes when tenant-scoped cache policies are wired in
//   ServiceCollectionExtensions.
//
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Analytics.Dashboard.BiDashboards.Authorization;
using PlantSafety.Api.Analytics.Dashboard.BiDashboards.Contracts.Requests;
using PlantSafety.Api.Analytics.Dashboard.BiDashboards.Contracts.Responses;
using PlantSafety.Application.Analytics.Dashboard.AuditDashboard.Models;
using PlantSafety.Application.Analytics.Dashboard.AuditDashboard.Queries;
using PlantSafety.Application.Analytics.Dashboard.ContinuousImprovementDashboard.Models;
using PlantSafety.Application.Analytics.Dashboard.ContinuousImprovementDashboard.Queries;
using PlantSafety.Application.Analytics.Dashboard.MaintenanceDashboard.Models;
using PlantSafety.Application.Analytics.Dashboard.MaintenanceDashboard.Queries;
using PlantSafety.Application.Analytics.Dashboard.ProductionDashboard.Models;
using PlantSafety.Application.Analytics.Dashboard.ProductionDashboard.Queries;
using PlantSafety.Application.Analytics.Dashboard.RiskDashboard.Models;
using PlantSafety.Application.Analytics.Dashboard.RiskDashboard.Queries;
using PlantSafety.Application.Analytics.Dashboard.SchedulingDashboard.Models;
using PlantSafety.Application.Analytics.Dashboard.SchedulingDashboard.Queries;

namespace PlantSafety.Api.Analytics.Dashboard.BiDashboards.Controllers.V1;

/// <summary>V1 controller for the six BI Dashboards KPI section read endpoints (Phase 5 §4.3).</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/analytics/dashboard")]
[Authorize(Policy = BiDashboardsAuthorizationPolicies.Read)]
[Tags("Analytics.Dashboard.BiDashboards")]
public sealed class BiDashboardsV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="BiDashboardsV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">
    /// Resolves the trusted, server-side TenantId from the authenticated JWT claims
    /// (Zero-Trust tenant isolation -- never trust a client-supplied TenantId).
    /// </param>
    public BiDashboardsV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Returns the Scheduling BI dashboard KPI section for the resolved tenant.</summary>
    /// <param name="request">Query parameters bound from the query string.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the Scheduling KPI result set.</returns>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("scheduling")]
    [ProducesResponseType(typeof(ApiResponse<BiDashboardResponse<SchedulingDashboardItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSchedulingAsync(
        [FromQuery] GetBiDashboardRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var correlationId = env.CorrelationId.ToString();

        var query = new GetSchedulingDashboardQuery(env.TenantId, request.SiteId, request.From, request.To ?? DateTime.UtcNow);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<BiDashboardResponse<SchedulingDashboardItemDto>>.Fail(result.Error.Message, correlationId));

        var response = new BiDashboardResponse<SchedulingDashboardItemDto>(
            result.Value.TenantId, result.Value.SiteId, result.Value.GeneratedAt, result.Value.Items);

        return Ok(ApiResponse<BiDashboardResponse<SchedulingDashboardItemDto>>.Ok(response, correlationId));
    }

    /// <summary>Returns the Production/Operational BI dashboard KPI section for the resolved tenant.</summary>
    /// <param name="request">Query parameters bound from the query string.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the Production KPI result set.</returns>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("production")]
    [ProducesResponseType(typeof(ApiResponse<BiDashboardResponse<ProductionDashboardItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetProductionAsync(
        [FromQuery] GetBiDashboardRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var correlationId = env.CorrelationId.ToString();

        var query = new GetProductionDashboardQuery(env.TenantId, request.SiteId, request.From, request.To ?? DateTime.UtcNow);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<BiDashboardResponse<ProductionDashboardItemDto>>.Fail(result.Error.Message, correlationId));

        var response = new BiDashboardResponse<ProductionDashboardItemDto>(
            result.Value.TenantId, result.Value.SiteId, result.Value.GeneratedAt, result.Value.Items);

        return Ok(ApiResponse<BiDashboardResponse<ProductionDashboardItemDto>>.Ok(response, correlationId));
    }

    /// <summary>Returns the Maintenance BI dashboard KPI section for the resolved tenant.</summary>
    /// <param name="request">Query parameters bound from the query string.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the Maintenance KPI result set.</returns>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("maintenance")]
    [ProducesResponseType(typeof(ApiResponse<BiDashboardResponse<MaintenanceDashboardItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMaintenanceAsync(
        [FromQuery] GetBiDashboardRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var correlationId = env.CorrelationId.ToString();

        var query = new GetMaintenanceDashboardQuery(env.TenantId, request.SiteId, request.From, request.To ?? DateTime.UtcNow);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<BiDashboardResponse<MaintenanceDashboardItemDto>>.Fail(result.Error.Message, correlationId));

        var response = new BiDashboardResponse<MaintenanceDashboardItemDto>(
            result.Value.TenantId, result.Value.SiteId, result.Value.GeneratedAt, result.Value.Items);

        return Ok(ApiResponse<BiDashboardResponse<MaintenanceDashboardItemDto>>.Ok(response, correlationId));
    }

    /// <summary>Returns the Risk BI dashboard KPI section for the resolved tenant.</summary>
    /// <param name="request">Query parameters bound from the query string.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the Risk KPI result set.</returns>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("risk")]
    [ProducesResponseType(typeof(ApiResponse<BiDashboardResponse<RiskDashboardItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRiskAsync(
        [FromQuery] GetBiDashboardRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var correlationId = env.CorrelationId.ToString();

        var query = new GetRiskDashboardQuery(env.TenantId, request.SiteId, request.From, request.To ?? DateTime.UtcNow);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<BiDashboardResponse<RiskDashboardItemDto>>.Fail(result.Error.Message, correlationId));

        var response = new BiDashboardResponse<RiskDashboardItemDto>(
            result.Value.TenantId, result.Value.SiteId, result.Value.GeneratedAt, result.Value.Items);

        return Ok(ApiResponse<BiDashboardResponse<RiskDashboardItemDto>>.Ok(response, correlationId));
    }

    /// <summary>Returns the ContinuousImprovement BI dashboard KPI section for the resolved tenant.</summary>
    /// <param name="request">Query parameters bound from the query string.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the ContinuousImprovement KPI result set.</returns>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("continuousimprovement")]
    [ProducesResponseType(typeof(ApiResponse<BiDashboardResponse<ContinuousImprovementDashboardItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetContinuousImprovementAsync(
        [FromQuery] GetBiDashboardRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var correlationId = env.CorrelationId.ToString();

        var query = new GetContinuousImprovementDashboardQuery(env.TenantId, request.SiteId, request.From, request.To ?? DateTime.UtcNow);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<BiDashboardResponse<ContinuousImprovementDashboardItemDto>>.Fail(result.Error.Message, correlationId));

        var response = new BiDashboardResponse<ContinuousImprovementDashboardItemDto>(
            result.Value.TenantId, result.Value.SiteId, result.Value.GeneratedAt, result.Value.Items);

        return Ok(ApiResponse<BiDashboardResponse<ContinuousImprovementDashboardItemDto>>.Ok(response, correlationId));
    }

    /// <summary>Returns the Audit BI dashboard KPI section for the resolved tenant.</summary>
    /// <param name="request">Query parameters bound from the query string.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the Audit KPI result set.</returns>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("audit")]
    [ProducesResponseType(typeof(ApiResponse<BiDashboardResponse<AuditDashboardItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAuditAsync(
        [FromQuery] GetBiDashboardRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var correlationId = env.CorrelationId.ToString();

        var query = new GetAuditDashboardQuery(env.TenantId, request.SiteId, request.From, request.To ?? DateTime.UtcNow);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<BiDashboardResponse<AuditDashboardItemDto>>.Fail(result.Error.Message, correlationId));

        var response = new BiDashboardResponse<AuditDashboardItemDto>(
            result.Value.TenantId, result.Value.SiteId, result.Value.GeneratedAt, result.Value.Items);

        return Ok(ApiResponse<BiDashboardResponse<AuditDashboardItemDto>>.Ok(response, correlationId));
    }
}
