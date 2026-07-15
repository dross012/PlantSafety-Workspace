// ============================================================================
// File: IncidentsCanonicalV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-15
// Description: V1 MVC controller exposing the canonical UI-facing REST surface for
//              Incident Management reads at api/v1/safety-events/incidents. Delegates
//              to the same Application-layer Features/SafetyEvents/IncidentManagement
//              queries used by PlantSafety.Api.Features.SafetyEvents.IncidentManagement
//              (see IncidentsV1Controller under Features/), but returns the raw payload
//              shape (PaginatedResult<T> / T) expected by PlantSafety.UI's
//              SafetyApiClientBase, instead of the ApiResponse<T> envelope used by the
//              legacy stub controllers historically generated under this folder.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/safety-events/incidents/...
// - This route is the canonical surface consumed by PlantSafety.UI.Modules.
//   IncidentManagement.ApiClients.IncidentManagementApiClient. It intentionally
//   coexists with the /api/v1/features/safety-events/incidents surface (which
//   also exposes Create/Update/Delete) — both dispatch the same MediatR queries.
//
// Rationale:
// - Routes all reads through MediatR ISender to the Application layer. No domain
//   logic or data access here.
//
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.SafetyEvents.IncidentManagement.Authorization;
using PlantSafety.Api.SafetyEvents.IncidentManagement.Contracts.Requests;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Features.SafetyEvents.Models.IncidentManagement;
using PlantSafety.Application.Features.SafetyEvents.Queries.IncidentManagement.Get;
using PlantSafety.Application.Features.SafetyEvents.Queries.IncidentManagement.List;

namespace PlantSafety.Api.SafetyEvents.IncidentManagement.Controllers.V1;

/// <summary>
/// V1 controller for the canonical Incident Management read surface consumed by
/// PlantSafety.UI (<c>v1/safety-events/incidents</c>).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/safety-events/incidents")]
[Authorize(Policy = IncidentManagementAuthorizationPolicies.Read)]
[Tags("SafetyEvents.Incidents")]
public sealed class IncidentsCanonicalV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="IncidentsCanonicalV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds canonical Command/Query envelope values for the current request.</param>
    public IncidentsCanonicalV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Returns a paginated, filterable list of Incidents for the caller's tenant.</summary>
    /// <param name="request">Filter and paging parameters bound from the query string.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>The canonical <see cref="PaginatedResult{T}"/> envelope wrapping Incident summaries.</returns>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<IncidentSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListAsync(
        [FromQuery] ListIncidentsCanonicalRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new ListIncidentsQuery
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
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            OrderBy = request.OrderBy,
            SortDirection = request.SortDirection,
            SiteId = request.SiteId,
            Status = request.Status,
            Severity = request.Severity,
            OccurredFromUtc = request.OccurredFromUtc,
            OccurredToUtc = request.OccurredToUtc
        };

        var result = await _sender.Send(query, cancellationToken);

        return Ok(result);
    }

    /// <summary>Returns the full Incident projection for a given identifier.</summary>
    /// <param name="incidentId">Incident to retrieve.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>The <see cref="IncidentDto"/> when found.</returns>
    /// <response code="200">Incident found.</response>
    /// <response code="404">Incident not found or soft-deleted.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("{incidentId:guid}")]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        [FromRoute] Guid incidentId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetIncidentByIdQuery
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
            IncidentId = incidentId
        };

        var incident = await _sender.Send(query, cancellationToken);

        if (incident is null)
            return NotFound();

        return Ok(incident);
    }
}
