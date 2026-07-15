// ============================================================================
// File: RoiV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-15
// Description: V1 controller exposing Financials/ROI Commands and Queries from the
//              Application-layer Features/ CQRS tree over HTTP.
// ============================================================================
//
// Onboarding Notes:
// - Routes follow /api/v1/features/financials/roi (matches the route already
//   coded into PlantSafety.UI's FinancialsApiClient.GetRoiAsync).
// - GET (no id segment) returns the most recent ROI calculation for the given
//   FiscalYear/SiteId; GET history returns paginated calculation history.
//
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Features.Financials.ROI.Contracts.Requests;
using PlantSafety.Api.Features.Financials.ROI.Contracts.Responses;
using PlantSafety.Application.Features.Financials.Commands.ROI.CalculateROI;
using PlantSafety.Application.Features.Financials.Models.ROI;
using PlantSafety.Application.Features.Financials.Queries.ROI.GetROI;
using PlantSafety.Application.Features.Financials.Queries.ROI.ListROIHistory;

namespace PlantSafety.Api.Features.Financials.ROI.Controllers.V1;

/// <summary>V1 controller for Financials/ROI command and query endpoints.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/features/financials/roi")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("ROI")]
public sealed class RoiV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="RoiV1Controller"/>.</summary>
    public RoiV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Records a new Safety ROI calculation.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CalculateRoiResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CalculateAsync(
        [FromBody] CalculateRoiRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CalculateRoiCommand
        {
            CommandId = env.CommandId,
            CorrelationId = env.CorrelationId,
            CausationId = env.CausationId,
            RequestedAtUtc = env.RequestedAtUtc,
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId,
            ActorId = env.ActorId,
            SiteId = request.SiteId,
            FiscalYear = request.FiscalYear,
            TotalInvestment = request.TotalInvestment,
            EstimatedAvoidedCosts = request.EstimatedAvoidedCosts,
            CurrencyCode = request.CurrencyCode,
        };

        var roiCalculationId = await _sender.Send(command, cancellationToken);

        return Ok(ApiResponse<CalculateRoiResponse>.Ok(new CalculateRoiResponse(roiCalculationId), env.CorrelationId.ToString()));
    }

    /// <summary>Returns the most recent Safety ROI calculation for the given fiscal year.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<RoiResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAsync(
        [FromQuery] int fiscalYear,
        [FromQuery] Guid? siteId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetRoiQuery
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
            FiscalYear = fiscalYear,
        };

        var roi = await _sender.Send(query, cancellationToken);

        if (roi is null)
            return NotFound(ApiResponse<RoiResponse>.Fail("ROI calculation not found for the given fiscal year.", env.CorrelationId.ToString()));

        return Ok(ApiResponse<RoiResponse>.Ok(ToResponse(roi), env.CorrelationId.ToString()));
    }

    /// <summary>Returns paginated Safety ROI calculation history for the tenant.</summary>
    [HttpGet("history")]
    [ProducesResponseType(typeof(ApiResponse<RoiHistoryListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListHistoryAsync(
        [FromQuery] int pageNumber,
        [FromQuery] int pageSize,
        [FromQuery] Guid? siteId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new ListRoiHistoryQuery
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
            PageNumber = pageNumber <= 0 ? 1 : pageNumber,
            PageSize = pageSize <= 0 ? 25 : pageSize,
            SiteId = siteId,
        };

        var result = await _sender.Send(query, cancellationToken);

        var response = new RoiHistoryListResponse(
            result.Items.Select(ToResponse).ToList(),
            result.TotalCount,
            result.PageNumber,
            result.PageSize,
            result.TotalPages,
            result.HasNextPage,
            result.HasPreviousPage);

        return Ok(ApiResponse<RoiHistoryListResponse>.Ok(response, env.CorrelationId.ToString()));
    }

    private static RoiResponse ToResponse(SafetyROIDto dto) => new(
        dto.RoiCalculationId,
        dto.TenantId,
        dto.SiteId,
        dto.FiscalYear,
        dto.TotalInvestment,
        dto.EstimatedAvoidedCosts,
        dto.NetBenefit,
        dto.RoiPercentage,
        dto.CurrencyCode,
        dto.CalculatedAtUtc);
}
