// ============================================================================
// File: RCAV1Controller.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-08-02
// Description: V1 MVC controller exposing GET endpoints for the RCA read-model (pre-existing)
//              plus the Phase 4 full build-out's Create/lifecycle/by-id command and query
//              endpoints for the newly built RCA aggregate.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow /api/v1/continuous-improvement/rca/...
//
// Rationale:
// - Routes all reads through MediatR ISender to the Application layer. No domain logic or data access here.
//
// Future Extension Seams:
// - Add [OutputCache(PolicyName = ...)] attributes when tenant-scoped cache policies are wired in ServiceCollectionExtensions.
//
// ============================================================================

using PlantSafety.Api.ContinuousImprovement.RCA.Contracts.Requests;
using PlantSafety.Api.ContinuousImprovement.RCA.Contracts.Responses;
using PlantSafety.Api.ContinuousImprovement.RCA.Authorization;
using PlantSafety.Application.ContinuousImprovement.RCA.Commands;
using PlantSafety.Application.ContinuousImprovement.RCA.Queries;

using PlantSafety.Api.Common;
using PlantSafety.Api.Context;

namespace PlantSafety.Api.ContinuousImprovement.RCA.Controllers.V1;

/// <summary>
/// V1 controller for the ContinuousImprovement.RCA module's read endpoints (pre-existing) plus
/// the Phase 4 full build-out's Create/lifecycle/by-id command and query endpoints.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/continuous-improvement/rca")]
[Authorize(Policy = RCAAuthorizationPolicies.Read)]
[Tags("RCA")]
public sealed class RCAV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="RCAV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public RCAV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Creates a new ContinuousImprovement RCA record.</summary>
    [HttpPost]
    [Authorize(Policy = RCAAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<RCAStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RCAStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateRCARequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateRCACommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            ProblemStatement: request.ProblemStatement,
            Methodology: request.Methodology,
            OwnerUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Begins analysis work on an RCA record (Initiated -&gt; InProgress).</summary>
    [HttpPost("{rcaId:guid}/start-analysis")]
    [Authorize(Policy = RCAAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<RCAStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RCAStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> StartAnalysisAsync(
        Guid rcaId,
        [FromBody] StartAnalysisRCARequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new StartAnalysisRCACommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            RcaId: rcaId,
            StartedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Submits an RCA record for approval (InProgress -&gt; PendingApproval).</summary>
    [HttpPost("{rcaId:guid}/submit-for-approval")]
    [Authorize(Policy = RCAAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<RCAStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RCAStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SubmitForApprovalAsync(
        Guid rcaId,
        [FromBody] SubmitForApprovalRCARequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new SubmitForApprovalRCACommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            RcaId: rcaId,
            SubmittedByUserId: env.ActorId,
            Findings: request?.Findings)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Approves and formally closes an RCA record (PendingApproval -&gt; Closed).</summary>
    [HttpPost("{rcaId:guid}/close")]
    [Authorize(Policy = RCAAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<RCAStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RCAStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CloseAsync(
        Guid rcaId,
        [FromBody] CloseRCARequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CloseRCACommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            RcaId: rcaId,
            ClosedByUserId: env.ActorId,
            RootCauseSummary: request?.RootCauseSummary)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Archives a closed RCA record for compliance retention (Closed -&gt; Archived).</summary>
    [HttpPost("{rcaId:guid}/archive")]
    [Authorize(Policy = RCAAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<RCAStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RCAStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ArchiveAsync(
        Guid rcaId,
        [FromBody] ArchiveRCARequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ArchiveRCACommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            RcaId: rcaId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single RCA read-model record by identity, including its current ETag.</summary>
    [HttpGet("{rcaId:guid}")]
    [Authorize(Policy = RCAAuthorizationPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.ContinuousImprovement.RCA.Models.RCADetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        Guid rcaId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetRCAByIdQuery(env.TenantId, rcaId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns RCA data for the resolved tenant.</summary>
    /// <param name="request">Query parameters bound from the query string.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the RCA result set.</returns>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<RCAResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAsync(
        [FromQuery] GetRCARequest request,
        CancellationToken cancellationToken)
    {
        var correlationId = HttpContext.Items[PlantSafety.Api.Middleware.CorrelationIdMiddleware.ItemsKey]?.ToString()
                            ?? string.Empty;

        var query  = new GetRCAQuery(request.TenantId, request.SiteId, request.From, request.To ?? DateTime.UtcNow);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<RCAResponse>.Fail(result.Error.Message, correlationId));

        var response = new RCAResponse(
            result.Value.TenantId,
            result.Value.SiteId,
            result.Value.GeneratedAt,
            result.Value.Items);

        return Ok(ApiResponse<RCAResponse>.Ok(response, correlationId));
    }
}