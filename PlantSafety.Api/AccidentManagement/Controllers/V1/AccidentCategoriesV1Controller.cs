// ============================================================================
// File: AccidentCategoriesV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (Claude Code modular-creation-agent)
// Created: 2026-07-21
// Description: V1 MVC controller exposing the 4 AccidentManagement category read-model queries
//              over HTTP as one GET action per category.
// ============================================================================

using PlantSafety.Api.AccidentManagement.Contracts.Requests;
using PlantSafety.Api.Common;
using PlantSafety.Api.Context;
using PlantSafety.Application.AccidentManagement.CorrectiveActionsCAPA.Queries;
using PlantSafety.Application.AccidentManagement.InvestigationRootCauseAnalysis.Queries;
using PlantSafety.Application.AccidentManagement.RegulatoryComplianceDocumentation.Queries;
using PlantSafety.Application.AccidentManagement.SiteSecuringAndTriage.Queries;

namespace PlantSafety.Api.AccidentManagement.Controllers.V1;

/// <summary>V1 controller for the 4 AccidentManagement category read-model queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/accidentmanagement/categories")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("AccidentManagement")]
public sealed class AccidentCategoriesV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="AccidentCategoriesV1Controller"/>.</summary>
    public AccidentCategoriesV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Returns CorrectiveActionsCAPA read-model data for the caller's tenant.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("corrective-actions-capa")]
    [ProducesResponseType(typeof(ApiResponse<CorrectiveActionsCAPAQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCorrectiveActionsCAPAAsync([FromQuery] CategoryQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetCorrectiveActionsCAPAQuery(env.TenantId, request.SiteId, request.From, request.To);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns InvestigationRootCauseAnalysis read-model data for the caller's tenant.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("investigation-root-cause-analysis")]
    [ProducesResponseType(typeof(ApiResponse<InvestigationRootCauseAnalysisQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetInvestigationRootCauseAnalysisAsync([FromQuery] CategoryQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetInvestigationRootCauseAnalysisQuery(env.TenantId, request.SiteId, request.From, request.To);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns RegulatoryComplianceDocumentation read-model data for the caller's tenant.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("regulatory-compliance-documentation")]
    [ProducesResponseType(typeof(ApiResponse<RegulatoryComplianceDocumentationQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRegulatoryComplianceDocumentationAsync([FromQuery] CategoryQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetRegulatoryComplianceDocumentationQuery(env.TenantId, request.SiteId, request.From, request.To);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns SiteSecuringAndTriage read-model data for the caller's tenant.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("site-securing-and-triage")]
    [ProducesResponseType(typeof(ApiResponse<SiteSecuringAndTriageQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSiteSecuringAndTriageAsync([FromQuery] CategoryQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetSiteSecuringAndTriageQuery(env.TenantId, request.SiteId, request.From, request.To);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
