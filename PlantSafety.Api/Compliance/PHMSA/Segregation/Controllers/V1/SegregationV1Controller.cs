// ============================================================================
// File: SegregationV1Controller.cs
// Project: PlantSafety.Api
// Module: Compliance.PHMSA.Segregation
// Created: 2026-08-07
// Description: V1 controller exposing the Application-layer Compliance.PHMSA.Segregation module's
//              queries over HTTP. There is no persisted aggregate for Segregation -- both actions
//              are pure calculations over a caller-supplied load payload, exposed as HTTP POST
//              (the payload is too large for a query string) but authorized as reads
//              (SafetyTierPolicies.Read) since they are side-effect-free. Mirrors
//              PlantSafety.Api.Compliance.Permits.Controllers.V1.PermitsV1Controller.
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Compliance.PHMSA.Segregation.Contracts.Requests;
using PlantSafety.Application.Compliance.PHMSA.Segregation.Queries;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.Compliance.PHMSA.Segregation.Controllers.V1;

/// <summary>V1 controller for the Compliance.PHMSA.Segregation module's queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/compliance/phmsa/segregation")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Compliance.PHMSA.Segregation")]
public sealed class SegregationV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="SegregationV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public SegregationV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Evaluates a proposed multi-hazmat load for segregation incompatibilities.</summary>
    [HttpPost("evaluate")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.Compliance.PHMSA.Segregation.Models.SegregationEvaluationResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.Compliance.PHMSA.Segregation.Models.SegregationEvaluationResultDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> EvaluateLoadAsync(
        [FromBody] EvaluateLoadRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new EvaluateLoadQuery(env.TenantId, request.LoadItems);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Suggests a recommended load plan for a proposed multi-hazmat load.</summary>
    [HttpPost("suggest-load-plan")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<SuggestLoadPlanResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SuggestLoadPlanResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SuggestLoadPlanAsync(
        [FromBody] SuggestLoadPlanRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new SuggestLoadPlanQuery(env.TenantId, request.LoadItems);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
