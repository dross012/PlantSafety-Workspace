// ============================================================================
// File: PredictiveSafetyAlertV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (Claude Code refactoring-agent)
// Created: 2026-08-02
// Description: V1 MVC controller exposing the Phase 4 full build-out's Create/lifecycle/by-id
//              command and query endpoints for the newly built PredictiveSafetyAlert aggregate.
//              Distinct from the pre-existing
//              PlantSafety.Api.Intelligence.PredictiveSafety.Controllers.V1
//              .PredictiveSafetyV1Controller (the untouched date-range read-model controller) --
//              different controller class name, different route segment ("alerts" vs the read
//              controller's own route), no path collision.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/intelligence/predictive-safety/alerts/...
//
// Rationale:
// - Routes all reads/writes through MediatR ISender to the Application layer. No domain logic or
//   data access here.
//
// ============================================================================

using PlantSafety.Api.Intelligence.PredictiveSafety.Contracts.Requests;
using PlantSafety.Api.Intelligence.PredictiveSafety.Authorization;
using PlantSafety.Application.Intelligence.PredictiveSafety.Commands;
using PlantSafety.Application.Intelligence.PredictiveSafety.Queries;

using PlantSafety.Api.Common;
using PlantSafety.Api.Context;

namespace PlantSafety.Api.Intelligence.PredictiveSafety.Controllers.V1;

/// <summary>
/// V1 controller for the Phase 4 full build-out's Create/lifecycle/by-id command and query
/// endpoints for the PredictiveSafetyAlert aggregate.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/intelligence/predictive-safety/alerts")]
[Authorize(Policy = PredictiveSafetyAuthorizationPolicies.Read)]
[Tags("PredictiveSafetyAlert")]
public sealed class PredictiveSafetyAlertV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="PredictiveSafetyAlertV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public PredictiveSafetyAlertV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Creates a new Intelligence.PredictiveSafety alert record.</summary>
    [HttpPost]
    [Authorize(Policy = PredictiveSafetyAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PredictiveSafetyAlertStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PredictiveSafetyAlertStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreatePredictiveSafetyAlertRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreatePredictiveSafetyAlertCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            RiskCategory: request.RiskCategory,
            PredictedRiskScore: request.PredictedRiskScore,
            ModelVersion: request.ModelVersion,
            GeneratedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Acknowledges a predictive safety alert (Active -&gt; Acknowledged).</summary>
    [HttpPost("{predictiveSafetyAlertId:guid}/acknowledge")]
    [Authorize(Policy = PredictiveSafetyAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PredictiveSafetyAlertStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PredictiveSafetyAlertStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AcknowledgeAsync(
        Guid predictiveSafetyAlertId,
        [FromBody] AcknowledgePredictiveSafetyAlertRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new AcknowledgePredictiveSafetyAlertCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PredictiveSafetyAlertId: predictiveSafetyAlertId,
            AcknowledgedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Resolves a predictive safety alert (Acknowledged -&gt; Resolved).</summary>
    [HttpPost("{predictiveSafetyAlertId:guid}/resolve")]
    [Authorize(Policy = PredictiveSafetyAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PredictiveSafetyAlertStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PredictiveSafetyAlertStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ResolveAsync(
        Guid predictiveSafetyAlertId,
        [FromBody] ResolvePredictiveSafetyAlertRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ResolvePredictiveSafetyAlertCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PredictiveSafetyAlertId: predictiveSafetyAlertId,
            ResolvedByUserId: env.ActorId,
            ResolutionNotes: request?.ResolutionNotes)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Archives a resolved predictive safety alert for compliance retention (Resolved -&gt; Archived).</summary>
    [HttpPost("{predictiveSafetyAlertId:guid}/archive")]
    [Authorize(Policy = PredictiveSafetyAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PredictiveSafetyAlertStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PredictiveSafetyAlertStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ArchiveAsync(
        Guid predictiveSafetyAlertId,
        [FromBody] ArchivePredictiveSafetyAlertRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ArchivePredictiveSafetyAlertCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PredictiveSafetyAlertId: predictiveSafetyAlertId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single PredictiveSafetyAlert read-model record by identity, including its current ETag.</summary>
    [HttpGet("{predictiveSafetyAlertId:guid}")]
    [Authorize(Policy = PredictiveSafetyAuthorizationPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.Intelligence.PredictiveSafety.Models.PredictiveSafetyAlertDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        Guid predictiveSafetyAlertId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetPredictiveSafetyAlertByIdQuery(env.TenantId, predictiveSafetyAlertId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
