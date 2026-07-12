// ============================================================================
// File: RootCauseAnalysisV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: V1 MVC controller exposing Create/Get endpoints for the Features/SafetyEvents
//              Root Cause Analysis command/query pair.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/features/safety-events/root-cause-analysis/...
// - Commands/Queries in Features/SafetyEvents carry the full correlation-lineage envelope
//   (§2.1-2.2 of the Enterprise Architectural Rule Handbook). RequestEnvelopeFactory builds
//   those fields from the current request's IOperationContext/ICurrentUserService.
//
// Rationale:
// - Routes all reads/writes through MediatR ISender to the Application layer. No domain
//   logic or data access here.
//
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Features.SafetyEvents.RootCauseAnalysis.Contracts.Requests;
using PlantSafety.Api.Features.SafetyEvents.RootCauseAnalysis.Contracts.Responses;
using PlantSafety.Application.Features.SafetyEvents.Commands.RootCauseAnalysis.Create;
using PlantSafety.Application.Features.SafetyEvents.Queries.RootCauseAnalysis.Get;

namespace PlantSafety.Api.Features.SafetyEvents.RootCauseAnalysis.Controllers.V1;

/// <summary>V1 controller for Root Cause Analysis command/query endpoints.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/features/safety-events/root-cause-analysis")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("RootCauseAnalysis")]
public sealed class RootCauseAnalysisV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="RootCauseAnalysisV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds Command/Query envelope fields from the current request.</param>
    public RootCauseAnalysisV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Initiates a Root Cause Analysis linked to an Incident or NearMiss.</summary>
    /// <param name="request">Request body describing the RCA to create.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the new RCA identifier.</returns>
    /// <response code="201">RCA created successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateRootCauseAnalysisResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateRootCauseAnalysisRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateRootCauseAnalysisCommand
        {
            CommandId = env.CommandId,
            CorrelationId = env.CorrelationId,
            CausationId = env.CausationId,
            RequestedAtUtc = env.RequestedAtUtc,
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId,
            ActorId = env.ActorId,
            LinkedEventId = request.LinkedEventId,
            LinkedEventType = request.LinkedEventType,
            Methodology = request.Methodology,
            LeadInvestigatorId = request.LeadInvestigatorId,
            RootCauses = request.RootCauses,
            CorrectiveActions = request.CorrectiveActions,
        };

        var rcaId = await _sender.Send(command, cancellationToken);

        var response = new CreateRootCauseAnalysisResponse(rcaId);

        return CreatedAtAction(
            nameof(GetByIdAsync),
            new { rcaId },
            ApiResponse<CreateRootCauseAnalysisResponse>.Ok(response, env.CorrelationId.ToString()));
    }

    /// <summary>Returns a single Root Cause Analysis by identifier.</summary>
    /// <param name="rcaId">RCA to retrieve.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the RCA record.</returns>
    /// <response code="200">RCA returned successfully.</response>
    /// <response code="404">No RCA exists with the given identifier.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("{rcaId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<RootCauseAnalysisResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RootCauseAnalysisResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        [FromRoute] Guid rcaId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetRcaByIdQuery
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
            RcaId = rcaId,
        };

        var rca = await _sender.Send(query, cancellationToken);

        if (rca is null)
            return NotFound(ApiResponse<RootCauseAnalysisResponse>.Fail(
                $"RCA '{rcaId}' was not found.", env.CorrelationId.ToString()));

        var response = new RootCauseAnalysisResponse(
            rca.RcaId,
            rca.TenantId,
            rca.LinkedEventId,
            rca.LinkedEventType,
            rca.Methodology,
            rca.RootCauses,
            rca.CorrectiveActions,
            rca.Status,
            rca.LeadInvestigatorId,
            rca.CompletedAtUtc,
            rca.LastModifiedAtUtc,
            rca.ETag);

        return Ok(ApiResponse<RootCauseAnalysisResponse>.Ok(response, env.CorrelationId.ToString()));
    }
}
