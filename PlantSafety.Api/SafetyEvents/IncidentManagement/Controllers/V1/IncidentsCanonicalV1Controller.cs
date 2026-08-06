// ============================================================================
// File: IncidentsCanonicalV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-15
// Description: V1 MVC controller exposing the canonical UI-facing REST surface for
//              Incident Management reads at api/v1/safety-events/incidents. Delegates
//              to the same Application-layer Features/SafetyEvents/IncidentManagement
//              queries used by PlantSafety.Api.Features.SafetyEvents.IncidentManagement
//              (see IncidentsV1Controller under Features/). Returns the canonical
//              ApiResponse<T> envelope like every other controller — PlantSafety.UI's
//              SafetyApiClientBase.ReadEnvelopeAsync unconditionally unwraps the
//              {success,data,errors,metadata} shape, so a raw payload here would
//              deserialize with Data always null. Corrected 2026-08-05 (was previously
//              returning raw Ok(payload), silently breaking every UI page that reads
//              from this controller).
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
using PlantSafety.Application.Features.SafetyEvents.Commands.IncidentManagement.AddNote;
using PlantSafety.Application.Features.SafetyEvents.Commands.IncidentManagement.Assign;
using PlantSafety.Application.Features.SafetyEvents.Commands.IncidentManagement.Escalate;
using PlantSafety.Application.Features.SafetyEvents.Commands.IncidentManagement.Resolve;
using PlantSafety.Application.Features.SafetyEvents.Models.IncidentManagement;
using PlantSafety.Application.Features.SafetyEvents.Queries.IncidentManagement.Get;
using PlantSafety.Application.Features.SafetyEvents.Queries.IncidentManagement.List;
using PlantSafety.Application.Features.SafetyEvents.Queries.IncidentManagement.Timeline;
using PlantSafety.Application.SafetyEvents.SlaMonitoring.Models;
using PlantSafety.Application.SafetyEvents.SlaMonitoring.Queries;

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

        return Ok(ApiResponse<PaginatedResult<IncidentSummaryDto>>.Ok(result, env.CorrelationId.ToString()));
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

        return Ok(ApiResponse<IncidentDto>.Ok(incident, env.CorrelationId.ToString()));
    }

    // ========================================================================
    // Lifecycle actions (assign/escalate/notes/resolve/timeline) -- added
    // 2026-08-03 (UI Gaps Scoping, IncidentManagement Part 1). Small addition to the existing
    // canonical incident aggregate: natural lifecycle commands alongside Create/Update above.
    // ========================================================================

    /// <summary>Returns the chronological timeline for a single incident.</summary>
    /// <param name="incidentId">Incident whose timeline is being retrieved.</param>
    /// <param name="pageNumber">1-based page number.</param>
    /// <param name="pageSize">Items per page.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("{incidentId:guid}/timeline")]
    [Authorize(Policy = IncidentManagementAuthorizationPolicies.Read)]
    [ProducesResponseType(typeof(PaginatedResult<IncidentTimelineEntryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTimelineAsync(
        [FromRoute] Guid incidentId,
        [FromQuery] int pageNumber,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetIncidentTimelineQuery
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
            IncidentId = incidentId,
            PageNumber = pageNumber == 0 ? 1 : pageNumber,
            PageSize = pageSize == 0 ? 25 : pageSize,
        };

        var result = await _sender.Send(query, cancellationToken);

        return Ok(ApiResponse<PaginatedResult<IncidentTimelineEntryDto>>.Ok(result, env.CorrelationId.ToString()));
    }

    /// <summary>Assigns (or reassigns) a technician/investigator to an incident.</summary>
    /// <param name="incidentId">Incident to assign.</param>
    /// <param name="request">Technician identifier bound from the request body.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("{incidentId:guid}/assign")]
    [Authorize(Policy = IncidentManagementAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignAsync(
        [FromRoute] Guid incidentId,
        [FromBody] AssignTechnicianRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new AssignTechnicianCommand
        {
            CommandId = env.CommandId,
            CorrelationId = env.CorrelationId,
            CausationId = env.CausationId,
            RequestedAtUtc = env.RequestedAtUtc,
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId,
            ActorId = env.ActorId,
            IncidentId = incidentId,
            TechnicianId = request.TechnicianId,
        };

        await _sender.Send(command, cancellationToken);

        return await GetUpdatedIncidentAsync(incidentId, cancellationToken);
    }

    /// <summary>Escalates an incident with a required reason.</summary>
    /// <param name="incidentId">Incident to escalate.</param>
    /// <param name="request">Escalation reason bound from the request body.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("{incidentId:guid}/escalate")]
    [Authorize(Policy = IncidentManagementAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EscalateAsync(
        [FromRoute] Guid incidentId,
        [FromBody] EscalateIncidentRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new EscalateIncidentCommand
        {
            CommandId = env.CommandId,
            CorrelationId = env.CorrelationId,
            CausationId = env.CausationId,
            RequestedAtUtc = env.RequestedAtUtc,
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId,
            ActorId = env.ActorId,
            IncidentId = incidentId,
            Reason = request.Reason,
        };

        await _sender.Send(command, cancellationToken);

        return await GetUpdatedIncidentAsync(incidentId, cancellationToken);
    }

    /// <summary>Adds a technician/operator note to an incident.</summary>
    /// <param name="incidentId">Incident to add a note to.</param>
    /// <param name="request">Note text bound from the request body.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("{incidentId:guid}/notes")]
    [Authorize(Policy = IncidentManagementAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddNoteAsync(
        [FromRoute] Guid incidentId,
        [FromBody] AddIncidentNoteRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new AddIncidentNoteCommand
        {
            CommandId = env.CommandId,
            CorrelationId = env.CorrelationId,
            CausationId = env.CausationId,
            RequestedAtUtc = env.RequestedAtUtc,
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId,
            ActorId = env.ActorId,
            IncidentId = incidentId,
            Text = request.Text,
        };

        await _sender.Send(command, cancellationToken);

        return await GetUpdatedIncidentAsync(incidentId, cancellationToken);
    }

    /// <summary>Resolves an incident with a required final resolution note.</summary>
    /// <param name="incidentId">Incident to resolve.</param>
    /// <param name="request">Resolution notes bound from the request body.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("{incidentId:guid}/resolve")]
    [Authorize(Policy = IncidentManagementAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResolveAsync(
        [FromRoute] Guid incidentId,
        [FromBody] ResolveIncidentRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ResolveIncidentCommand
        {
            CommandId = env.CommandId,
            CorrelationId = env.CorrelationId,
            CausationId = env.CausationId,
            RequestedAtUtc = env.RequestedAtUtc,
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId,
            ActorId = env.ActorId,
            IncidentId = incidentId,
            ResolutionNotes = request.ResolutionNotes,
        };

        await _sender.Send(command, cancellationToken);

        return await GetUpdatedIncidentAsync(incidentId, cancellationToken);
    }

    /// <summary>Re-fetches the incident after a lifecycle command so callers get the current projection.</summary>
    private async Task<IActionResult> GetUpdatedIncidentAsync(Guid incidentId, CancellationToken cancellationToken)
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
            IncidentId = incidentId,
        };

        var incident = await _sender.Send(query, cancellationToken);

        return incident is null
            ? NotFound()
            : Ok(ApiResponse<IncidentDto>.Ok(incident, env.CorrelationId.ToString()));
    }

    // ========================================================================
    // SLA Monitor (genuinely new SLA policy engine) -- added 2026-08-03 (UI Gaps Scoping,
    // IncidentManagement Part 2). Routed under this same controller's /sla-monitor sub-path so
    // it stays a sibling of the incident it monitors rather than a second route family.
    // ========================================================================

    /// <summary>Returns the SLA countdown grid rows for all active incidents.</summary>
    /// <param name="pageNumber">1-based page number.</param>
    /// <param name="pageSize">Items per page.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("sla-monitor/incidents")]
    [Authorize(Policy = IncidentManagementAuthorizationPolicies.Read)]
    [ProducesResponseType(typeof(PaginatedResult<SlaIncidentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSlaMonitorIncidentsAsync(
        [FromQuery] int pageNumber,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetSlaMonitorIncidentsQuery
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
            PageNumber = pageNumber == 0 ? 1 : pageNumber,
            PageSize = pageSize == 0 ? 25 : pageSize,
        };

        var result = await _sender.Send(query, cancellationToken);

        return Ok(ApiResponse<PaginatedResult<SlaIncidentDto>>.Ok(result, env.CorrelationId.ToString()));
    }

    /// <summary>Returns the SLA risk panel rows (incidents classified AtRisk or Breached).</summary>
    /// <param name="pageNumber">1-based page number.</param>
    /// <param name="pageSize">Items per page.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("sla-monitor/risk")]
    [Authorize(Policy = IncidentManagementAuthorizationPolicies.Read)]
    [ProducesResponseType(typeof(PaginatedResult<SlaRiskDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSlaMonitorRiskAsync(
        [FromQuery] int pageNumber,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetSlaMonitorRiskQuery
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
            PageNumber = pageNumber == 0 ? 1 : pageNumber,
            PageSize = pageSize == 0 ? 25 : pageSize,
        };

        var result = await _sender.Send(query, cancellationToken);

        return Ok(ApiResponse<PaginatedResult<SlaRiskDto>>.Ok(result, env.CorrelationId.ToString()));
    }

    /// <summary>Returns the SLA compliance trend summary for the trend chart.</summary>
    /// <param name="trendWindowDays">Rolling trend window, in days.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("sla-monitor/trend")]
    [Authorize(Policy = IncidentManagementAuthorizationPolicies.Read)]
    [ProducesResponseType(typeof(SlaTrendDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSlaMonitorTrendAsync(
        [FromQuery] int trendWindowDays,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetSlaMonitorTrendQuery
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
            TrendWindowDays = trendWindowDays == 0 ? 30 : trendWindowDays,
        };

        var result = await _sender.Send(query, cancellationToken);

        return Ok(ApiResponse<SlaTrendDto>.Ok(result, env.CorrelationId.ToString()));
    }
}
