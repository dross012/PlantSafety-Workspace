// ============================================================================
// File: AuditTimelineV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (API Contract Validation Agent)
// Created: 2026-08-02
// Last Updated: 2026-08-02
// Description: V1 controller exposing GET api/v1/auditing/audit-timeline, wrapping
//              GetAuditTimelineForEntityQuery (Phase 5 §6.1). Mirrors
//              PlantSafety.Api.Analytics.Dashboard.Executive.Controllers.V1.ExecutiveV1Controller's
//              single-GET read-model convention.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Route matches
//   PlantSafety.UI.Modules.Auditing.ApiClients.AuditingApiClient.GetAuditTimelineAsync exactly:
//   GET api/v1/auditing/audit-timeline?TenantId=&EntityType=&EntityId=
//
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Auditing.AuditTimeline.Authorization;
using PlantSafety.Api.Auditing.AuditTimeline.Contracts.Requests;
using PlantSafety.Application.Auditing.AuditTimeline.Models;
using PlantSafety.Application.Auditing.AuditTimeline.Queries;

namespace PlantSafety.Api.Auditing.AuditTimeline.Controllers.V1;

/// <summary>V1 controller for the Auditing.AuditTimeline read endpoint.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auditing/audit-timeline")]
[Authorize(Policy = AuditTimelineAuthorizationPolicies.Read)]
[Tags("Auditing.AuditTimeline")]
public sealed class AuditTimelineV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="AuditTimelineV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Resolves the trusted, server-side TenantId from authenticated JWT claims.</param>
    public AuditTimelineV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Reconstructs the full chronological AuditTimeline for a single entity/aggregate.</summary>
    /// <param name="request">Query parameters bound from the query string.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the composed AuditTimeline.</returns>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<AuditTimelineDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAsync(
        [FromQuery] GetAuditTimelineRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var correlationId = env.CorrelationId.ToString();

        if (string.IsNullOrWhiteSpace(request.EntityType))
            return BadRequest(ApiResponse<AuditTimelineDto>.Fail("EntityType is required.", correlationId));

        var query = new GetAuditTimelineForEntityQuery(env.TenantId, request.EntityType, request.EntityId);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<AuditTimelineDto>.Fail(result.Error.Message, correlationId));

        return Ok(ApiResponse<AuditTimelineDto>.Ok(result.Value, correlationId));
    }
}
