// ============================================================================
// File: RulesEngineV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (API Contract Validation Agent)
// Created: 2026-08-02
// Last Updated: 2026-08-02
// Description: V1 controller exposing the Compliance.RulesEngine sub-feature (Phase 5 §6.3):
//              EvaluateComplianceCommand, GetComplianceEvaluationQuery, and
//              GetComplianceEvaluationHistoryQuery.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes match
//   PlantSafety.UI.Modules.Compliance.ApiClients.ComplianceApiClient's RulesEngine methods exactly:
//   POST api/v1/compliance/rules-engine/evaluate?TenantId=
//   GET  api/v1/compliance/rules-engine/evaluations/{complianceEvaluationId}?TenantId=
//   GET  api/v1/compliance/rules-engine/evaluations/history?TenantId=&TargetEntityType=&TargetEntityId=
//
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Common;
using PlantSafety.Api.Compliance.RulesEngine.Authorization;
using PlantSafety.Api.Compliance.RulesEngine.Contracts.Requests;
using PlantSafety.Application.Compliance.RulesEngine.Commands;
using PlantSafety.Application.Compliance.RulesEngine.Models;
using PlantSafety.Application.Compliance.RulesEngine.Queries;

namespace PlantSafety.Api.Compliance.RulesEngine.Controllers.V1;

/// <summary>V1 controller for the Compliance.RulesEngine sub-feature.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/compliance/rules-engine")]
[Authorize(Policy = RulesEngineAuthorizationPolicies.Read)]
[Tags("Compliance.RulesEngine")]
public sealed class RulesEngineV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="RulesEngineV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public RulesEngineV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Runs the Compliance Rules Engine (Safety/Regulatory/InternalPolicy pipelines) against a target entity.</summary>
    /// <param name="request">Caller-supplied evaluation fields.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <remarks>
    /// TenantId is intentionally NOT bound from the client. It is resolved server-side from the
    /// authenticated JWT claims via <see cref="RequestEnvelopeFactory"/>/<c>IOperationContext</c>,
    /// per the Infrastructure/Security manual's Zero-Trust tenant-isolation requirement. A
    /// client-supplied <c>TenantId</c> query parameter, if present on the wire, is ignored.
    /// </remarks>
    [HttpPost("evaluate")]
    [Authorize(Policy = RulesEngineAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<ComplianceEvaluationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ComplianceEvaluationDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> EvaluateAsync(
        [FromBody] EvaluateComplianceRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = EvaluateComplianceCommand.FromDto(
            tenantId: env.TenantId,
            siteId: request.SiteId,
            triggeredBy: env.ActorId,
            targetEntityType: request.TargetEntityType,
            targetEntityId: request.TargetEntityId,
            categories: request.Categories,
            facts: request.Facts,
            correlationId: env.CorrelationId,
            causationId: env.CausationId,
            apiVersionToken: env.ApiVersionToken);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves a single ComplianceEvaluation run by identity.</summary>
    /// <param name="complianceEvaluationId">Identity of the evaluation to retrieve.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("evaluations/{complianceEvaluationId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ComplianceEvaluationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        Guid complianceEvaluationId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetComplianceEvaluationQuery(env.TenantId, complianceEvaluationId);
        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves the ComplianceEvaluation history for a target entity, most recent first.</summary>
    /// <param name="targetEntityType">Type discriminator of the entity.</param>
    /// <param name="targetEntityId">Identity of the entity.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("evaluations/history")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ComplianceEvaluationDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ComplianceEvaluationDto>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetHistoryAsync(
        [FromQuery] string targetEntityType,
        [FromQuery] Guid targetEntityId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        if (string.IsNullOrWhiteSpace(targetEntityType))
            return BadRequest(ApiResponse<IReadOnlyList<ComplianceEvaluationDto>>.Fail("TargetEntityType is required.", env.CorrelationId.ToString()));

        var query = new GetComplianceEvaluationHistoryQuery(env.TenantId, targetEntityType, targetEntityId);
        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
