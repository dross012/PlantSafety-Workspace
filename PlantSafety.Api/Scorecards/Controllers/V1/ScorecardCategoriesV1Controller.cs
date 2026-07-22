// ============================================================================
// File: ScorecardCategoriesV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (Claude Code modular-creation-agent)
// Created: 2026-07-21
// Description: V1 MVC controller exposing the 6 Scorecards category read-model queries over
//              HTTP as one GET action per category.
// ============================================================================

using PlantSafety.Api.Common;
using PlantSafety.Api.Context;
using PlantSafety.Api.Scorecards.Contracts.Requests;
using PlantSafety.Application.Scorecards.CulturalPrecursorMetrics.Queries;
using PlantSafety.Application.Scorecards.LaggingIndicators.Queries;
using PlantSafety.Application.Scorecards.LeadingIndicators.Queries;
using PlantSafety.Application.Scorecards.ReportingDistribution.Queries;
using PlantSafety.Application.Scorecards.ScorecardDataSources.Queries;
using PlantSafety.Application.Scorecards.ScorecardStructureVisualization.Queries;

namespace PlantSafety.Api.Scorecards.Controllers.V1;

/// <summary>V1 controller for the 6 Scorecards category read-model queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/scorecards/categories")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Scorecards")]
public sealed class ScorecardCategoriesV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="ScorecardCategoriesV1Controller"/>.</summary>
    public ScorecardCategoriesV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Returns CulturalPrecursorMetrics read-model data for the caller's tenant.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("cultural-precursor-metrics")]
    [ProducesResponseType(typeof(ApiResponse<CulturalPrecursorMetricsQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCulturalPrecursorMetricsAsync([FromQuery] CategoryQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetCulturalPrecursorMetricsQuery(env.TenantId, request.SiteId, request.From, request.To);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns LaggingIndicators read-model data for the caller's tenant.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("lagging-indicators")]
    [ProducesResponseType(typeof(ApiResponse<LaggingIndicatorsQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetLaggingIndicatorsAsync([FromQuery] CategoryQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetLaggingIndicatorsQuery(env.TenantId, request.SiteId, request.From, request.To);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns LeadingIndicators read-model data for the caller's tenant.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("leading-indicators")]
    [ProducesResponseType(typeof(ApiResponse<LeadingIndicatorsQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetLeadingIndicatorsAsync([FromQuery] CategoryQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetLeadingIndicatorsQuery(env.TenantId, request.SiteId, request.From, request.To);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns ReportingDistribution read-model data for the caller's tenant.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("reporting-distribution")]
    [ProducesResponseType(typeof(ApiResponse<ReportingDistributionQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetReportingDistributionAsync([FromQuery] CategoryQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetReportingDistributionQuery(env.TenantId, request.SiteId, request.From, request.To);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns ScorecardDataSources read-model data for the caller's tenant.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("scorecard-data-sources")]
    [ProducesResponseType(typeof(ApiResponse<ScorecardDataSourcesQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetScorecardDataSourcesAsync([FromQuery] CategoryQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetScorecardDataSourcesQuery(env.TenantId, request.SiteId, request.From, request.To);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns ScorecardStructureVisualization read-model data for the caller's tenant.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("scorecard-structure-visualization")]
    [ProducesResponseType(typeof(ApiResponse<ScorecardStructureVisualizationQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetScorecardStructureVisualizationAsync([FromQuery] CategoryQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetScorecardStructureVisualizationQuery(env.TenantId, request.SiteId, request.From, request.To);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
