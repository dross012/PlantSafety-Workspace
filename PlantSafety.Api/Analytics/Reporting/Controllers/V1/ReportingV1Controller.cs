// ============================================================================
// File: ReportingV1Controller.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: V1 MVC controller exposing GET endpoints for the Reporting read-model.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow /api/v1/analytics/reporting/...
//
// Rationale:
// - Routes all reads through MediatR ISender to the Application layer. No domain logic or data access here.
//
// Future Extension Seams:
// - Add [OutputCache(PolicyName = ...)] attributes when tenant-scoped cache policies are wired in ServiceCollectionExtensions.
//
// ============================================================================

using PlantSafety.Api.Analytics.Reporting.Contracts.Requests;
using PlantSafety.Api.Analytics.Reporting.Contracts.Responses;
using PlantSafety.Api.Analytics.Reporting.Authorization;
using PlantSafety.Api.Context;
using PlantSafety.Application.Analytics.Reporting.Models;
using PlantSafety.Application.Analytics.Reporting.Queries;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Api.Analytics.Reporting.Controllers.V1;

/// <summary>V1 controller for Reporting read endpoints.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/analytics/reporting")]
[Authorize(Policy = ReportingAuthorizationPolicies.Read)]
[Tags("Reporting")]
public sealed class ReportingV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="ReportingV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">
    /// Builds the canonical query envelope fields (incl. server-resolved TenantId) for the
    /// report-catalog endpoints added 2026-08-03. The pre-existing <see cref="GetAsync"/>
    /// action intentionally continues to bind TenantId from the query string per its original
    /// contract; not changed here to avoid an unreviewed breaking change to that endpoint.
    /// </param>
    public ReportingV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Returns Reporting data for the resolved tenant.</summary>
    /// <param name="request">Query parameters bound from the query string.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the Reporting result set.</returns>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<ReportingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAsync(
        [FromQuery] GetReportingRequest request,
        CancellationToken cancellationToken)
    {
        var correlationId = HttpContext.Items[PlantSafety.Api.Middleware.CorrelationIdMiddleware.ItemsKey]?.ToString()
                            ?? string.Empty;

        var query  = new GetReportingQuery(request.TenantId, request.SiteId, request.From, request.To ?? DateTime.UtcNow);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<ReportingResponse>.Fail(result.Error.Message, correlationId));

        var response = new ReportingResponse(
            result.Value.TenantId,
            result.Value.SiteId,
            result.Value.GeneratedAt,
            result.Value.Items);

        return Ok(ApiResponse<ReportingResponse>.Ok(response, correlationId));
    }

    /// <summary>
    /// Returns a paginated, optionally type-filtered page of the report catalog. Backs the
    /// PlantSafety.UI Reporting module's Reports List page.
    /// </summary>
    /// <param name="request">Filters and paging bound from the query string.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the paginated result set.</returns>
    /// <response code="200">Reports returned successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("reports")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<ReportListItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetReportsAsync(
        [FromQuery] ListReportsRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query  = new GetReportsPagedQuery(env.TenantId, request.SiteId, request.ReportType, request.PageNumber, request.PageSize);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<PaginatedResult<ReportListItemDto>>.Fail(result.Error.Message, env.CorrelationId.ToString()));

        return Ok(ApiResponse<PaginatedResult<ReportListItemDto>>.Ok(result.Value, env.CorrelationId.ToString()));
    }

    /// <summary>
    /// Returns a single report's full detail by identifier. Backs the PlantSafety.UI Reporting
    /// module's Report Detail page.
    /// </summary>
    /// <param name="reportId">Report to retrieve.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the <see cref="ReportDetailDto"/>.</returns>
    /// <response code="200">Report found.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Report not found.</response>
    [HttpGet("reports/{reportId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ReportDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<ReportDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReportByIdAsync(
        Guid reportId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query  = new GetReportByIdQuery(env.TenantId, reportId);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            var failResponse = ApiResponse<ReportDetailDto>.Fail(result.Error.Message, env.CorrelationId.ToString());
            return result.Error.Severity == ErrorSeverity.NotFound
                ? NotFound(failResponse)
                : BadRequest(failResponse);
        }

        return Ok(ApiResponse<ReportDetailDto>.Ok(result.Value, env.CorrelationId.ToString()));
    }
}