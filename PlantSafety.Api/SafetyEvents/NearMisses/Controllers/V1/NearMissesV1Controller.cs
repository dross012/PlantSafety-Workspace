// ============================================================================
// File: NearMissesV1Controller.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: V1 MVC controller exposing GET endpoints for the NearMisses read-model.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow /api/v1/safety-events/near-misses/...
//
// Rationale:
// - Routes all reads through MediatR ISender to the Application layer. No domain logic or data access here.
//
// Future Extension Seams:
// - Add [OutputCache(PolicyName = ...)] attributes when tenant-scoped cache policies are wired in ServiceCollectionExtensions.
//
// ============================================================================

using PlantSafety.Api.SafetyEvents.NearMisses.Contracts.Requests;
using PlantSafety.Api.SafetyEvents.NearMisses.Contracts.Responses;
using PlantSafety.Api.SafetyEvents.NearMisses.Authorization;
using PlantSafety.Application.SafetyEvents.NearMisses.Commands;
using PlantSafety.Application.SafetyEvents.NearMisses.Queries;

using PlantSafety.Api.Common;
using PlantSafety.Api.Context;

namespace PlantSafety.Api.SafetyEvents.NearMisses.Controllers.V1;

/// <summary>
/// V1 controller for the SafetyEvents.NearMisses module's read endpoints (pre-existing) plus the
/// Phase 4 full build-out's Create/lifecycle/by-id command and query endpoints.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/safety-events/near-misses")]
[Authorize(Policy = NearMissesAuthorizationPolicies.Read)]
[Tags("NearMisses")]
public sealed class NearMissesV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="NearMissesV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public NearMissesV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Reports a new NearMisses record.</summary>
    [HttpPost]
    [Authorize(Policy = NearMissesAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<NearMissesStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<NearMissesStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateNearMissesRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateNearMissesCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            Category: request.Category,
            PotentialSeverity: request.PotentialSeverity,
            ReportedAtUtc: request.ReportedAtUtc ?? DateTime.UtcNow,
            ReportedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Starts review of a NearMisses record (Reported -&gt; UnderReview).</summary>
    [HttpPost("{nearMissesId:guid}/start-review")]
    [Authorize(Policy = NearMissesAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<NearMissesStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<NearMissesStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> StartReviewAsync(
        Guid nearMissesId,
        [FromBody] StartReviewNearMissesRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new StartReviewNearMissesCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            NearMissesId: nearMissesId,
            ReviewedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Completes review of a NearMisses record and records findings (UnderReview -&gt; Completed).</summary>
    [HttpPost("{nearMissesId:guid}/complete")]
    [Authorize(Policy = NearMissesAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<NearMissesStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<NearMissesStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CompleteAsync(
        Guid nearMissesId,
        [FromBody] CompleteNearMissesRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CompleteNearMissesCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            NearMissesId: nearMissesId,
            CompletedByUserId: env.ActorId,
            Findings: request?.Findings)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Formally closes a completed NearMisses record (Completed -&gt; Closed).</summary>
    [HttpPost("{nearMissesId:guid}/close")]
    [Authorize(Policy = NearMissesAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<NearMissesStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<NearMissesStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CloseAsync(
        Guid nearMissesId,
        [FromBody] CloseNearMissesRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CloseNearMissesCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            NearMissesId: nearMissesId,
            ClosedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Archives a closed NearMisses record for compliance retention (Closed -&gt; Archived).</summary>
    [HttpPost("{nearMissesId:guid}/archive")]
    [Authorize(Policy = NearMissesAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<NearMissesStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<NearMissesStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ArchiveAsync(
        Guid nearMissesId,
        [FromBody] ArchiveNearMissesRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ArchiveNearMissesCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            NearMissesId: nearMissesId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single NearMisses read-model record by identity, including its current ETag.</summary>
    [HttpGet("{nearMissesId:guid}")]
    [Authorize(Policy = NearMissesAuthorizationPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.SafetyEvents.NearMisses.Models.NearMissesDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        Guid nearMissesId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetNearMissesByIdQuery(env.TenantId, nearMissesId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns NearMisses data for the resolved tenant.</summary>
    /// <param name="request">Query parameters bound from the query string.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the NearMisses result set.</returns>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<NearMissesResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAsync(
        [FromQuery] GetNearMissesRequest request,
        CancellationToken cancellationToken)
    {
        var correlationId = HttpContext.Items[PlantSafety.Api.Middleware.CorrelationIdMiddleware.ItemsKey]?.ToString()
                            ?? string.Empty;

        var query  = new GetNearMissesQuery(request.TenantId, request.SiteId, request.From, request.To ?? DateTime.UtcNow);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<NearMissesResponse>.Fail(result.Error.Message, correlationId));

        var response = new NearMissesResponse(
            result.Value.TenantId,
            result.Value.SiteId,
            result.Value.GeneratedAt,
            result.Value.Items);

        return Ok(ApiResponse<NearMissesResponse>.Ok(response, correlationId));
    }
}