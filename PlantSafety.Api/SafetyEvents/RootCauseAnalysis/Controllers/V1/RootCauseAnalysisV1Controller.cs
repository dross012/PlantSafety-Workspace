// ============================================================================
// File: RootCauseAnalysisV1Controller.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: V1 MVC controller exposing GET endpoints for the RootCauseAnalysis read-model.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow /api/v1/safety-events/root-cause-analysis/...
//
// Rationale:
// - Routes all reads through MediatR ISender to the Application layer. No domain logic or data access here.
//
// Future Extension Seams:
// - Add [OutputCache(PolicyName = ...)] attributes when tenant-scoped cache policies are wired in ServiceCollectionExtensions.
//
// ============================================================================

using PlantSafety.Api.SafetyEvents.RootCauseAnalysis.Contracts.Requests;
using PlantSafety.Api.SafetyEvents.RootCauseAnalysis.Contracts.Responses;
using PlantSafety.Api.SafetyEvents.RootCauseAnalysis.Authorization;
using PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Commands;
using PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Queries;

using PlantSafety.Api.Common;
using PlantSafety.Api.Context;

namespace PlantSafety.Api.SafetyEvents.RootCauseAnalysis.Controllers.V1;

/// <summary>
/// V1 controller for the SafetyEvents.RootCauseAnalysis module's read endpoints (pre-existing)
/// plus the Phase 4 full build-out's Create/lifecycle/by-id command and query endpoints.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/safety-events/root-cause-analysis")]
[Authorize(Policy = RootCauseAnalysisAuthorizationPolicies.Read)]
[Tags("RootCauseAnalysis")]
public sealed class RootCauseAnalysisV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="RootCauseAnalysisV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public RootCauseAnalysisV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Starts a new RootCauseAnalysis engagement.</summary>
    [HttpPost]
    [Authorize(Policy = RootCauseAnalysisAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<RootCauseAnalysisStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RootCauseAnalysisStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateRootCauseAnalysisRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateRootCauseAnalysisCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            RelatedEventId: request.RelatedEventId,
            Methodology: request.Methodology,
            LeadAnalystUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Submits a RootCauseAnalysis for review (InProgress -&gt; PendingReview).</summary>
    [HttpPost("{rootCauseAnalysisId:guid}/submit-for-review")]
    [Authorize(Policy = RootCauseAnalysisAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<RootCauseAnalysisStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RootCauseAnalysisStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SubmitForReviewAsync(
        Guid rootCauseAnalysisId,
        [FromBody] SubmitForReviewRootCauseAnalysisRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new SubmitForReviewRootCauseAnalysisCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            RootCauseAnalysisId: rootCauseAnalysisId,
            SubmittedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Reviews and approves a RootCauseAnalysis (PendingReview -&gt; Completed).</summary>
    [HttpPost("{rootCauseAnalysisId:guid}/approve")]
    [Authorize(Policy = RootCauseAnalysisAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<RootCauseAnalysisStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RootCauseAnalysisStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ApproveAsync(
        Guid rootCauseAnalysisId,
        [FromBody] ApproveRootCauseAnalysisRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ApproveRootCauseAnalysisCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            RootCauseAnalysisId: rootCauseAnalysisId,
            ApprovedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Formally closes a completed RootCauseAnalysis (Completed -&gt; Closed).</summary>
    [HttpPost("{rootCauseAnalysisId:guid}/close")]
    [Authorize(Policy = RootCauseAnalysisAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<RootCauseAnalysisStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RootCauseAnalysisStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CloseAsync(
        Guid rootCauseAnalysisId,
        [FromBody] CloseRootCauseAnalysisRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CloseRootCauseAnalysisCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            RootCauseAnalysisId: rootCauseAnalysisId,
            ClosedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Archives a closed RootCauseAnalysis for compliance retention (Closed -&gt; Archived).</summary>
    [HttpPost("{rootCauseAnalysisId:guid}/archive")]
    [Authorize(Policy = RootCauseAnalysisAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<RootCauseAnalysisStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RootCauseAnalysisStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ArchiveAsync(
        Guid rootCauseAnalysisId,
        [FromBody] ArchiveRootCauseAnalysisRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ArchiveRootCauseAnalysisCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            RootCauseAnalysisId: rootCauseAnalysisId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single RootCauseAnalysis read-model record by identity, including its current ETag.</summary>
    [HttpGet("{rootCauseAnalysisId:guid}")]
    [Authorize(Policy = RootCauseAnalysisAuthorizationPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Models.RootCauseAnalysisDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        Guid rootCauseAnalysisId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetRootCauseAnalysisByIdQuery(env.TenantId, rootCauseAnalysisId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns RootCauseAnalysis data for the resolved tenant.</summary>
    /// <param name="request">Query parameters bound from the query string.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the RootCauseAnalysis result set.</returns>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<RootCauseAnalysisResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAsync(
        [FromQuery] GetRootCauseAnalysisRequest request,
        CancellationToken cancellationToken)
    {
        var correlationId = HttpContext.Items[PlantSafety.Api.Middleware.CorrelationIdMiddleware.ItemsKey]?.ToString()
                            ?? string.Empty;

        var query  = new GetRootCauseAnalysisQuery(request.TenantId, request.SiteId, request.From, request.To ?? DateTime.UtcNow);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<RootCauseAnalysisResponse>.Fail(result.Error.Message, correlationId));

        var response = new RootCauseAnalysisResponse(
            result.Value.TenantId,
            result.Value.SiteId,
            result.Value.GeneratedAt,
            result.Value.Items);

        return Ok(ApiResponse<RootCauseAnalysisResponse>.Ok(response, correlationId));
    }
}