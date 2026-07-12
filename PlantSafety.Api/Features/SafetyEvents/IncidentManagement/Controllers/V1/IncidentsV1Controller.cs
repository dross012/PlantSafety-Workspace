// ============================================================================
// File: IncidentsV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: V1 MVC controller exposing the Application-layer Features/SafetyEvents
//              IncidentManagement Commands/Queries over HTTP.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/features/safety-events/incidents/...
// - Envelope fields (CommandId/CorrelationId/TraceId/etc.) are populated via
//   RequestEnvelopeFactory rather than hand-rolled from HttpContext.Items.
//
// Rationale:
// - Routes all reads/writes through MediatR ISender to the Application layer.
//   No domain logic or data access here.
//
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Features.SafetyEvents.IncidentManagement.Contracts.Requests;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Features.SafetyEvents.Commands.IncidentManagement.Create;
using PlantSafety.Application.Features.SafetyEvents.Commands.IncidentManagement.Delete;
using PlantSafety.Application.Features.SafetyEvents.Commands.IncidentManagement.Update;
using PlantSafety.Application.Features.SafetyEvents.Models.IncidentManagement;
using PlantSafety.Application.Features.SafetyEvents.Queries.IncidentManagement.Export;
using PlantSafety.Application.Features.SafetyEvents.Queries.IncidentManagement.Get;
using PlantSafety.Application.Features.SafetyEvents.Queries.IncidentManagement.List;
using PlantSafety.Application.Features.SafetyEvents.Queries.IncidentManagement.Search;

namespace PlantSafety.Api.Features.SafetyEvents.IncidentManagement.Controllers.V1;

/// <summary>V1 controller for Incident Management Commands and Queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/features/safety-events/incidents")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("SafetyEvents.Incidents")]
public sealed class IncidentsV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="IncidentsV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds canonical Command/Query envelope values for the current request.</param>
    public IncidentsV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Creates a new Incident.</summary>
    /// <param name="request">Incident payload bound from the request body.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the new Incident's identifier.</returns>
    /// <response code="201">Incident created successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateIncidentRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateIncidentCommand
        {
            CommandId = env.CommandId,
            CorrelationId = env.CorrelationId,
            CausationId = env.CausationId,
            RequestedAtUtc = env.RequestedAtUtc,
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId,
            ActorId = env.ActorId,
            SiteId = request.SiteId,
            Title = request.Title,
            Description = request.Description,
            IncidentOccurredAtUtc = request.IncidentOccurredAtUtc,
            Severity = request.Severity,
            Location = request.Location,
            ReportedByWorkerId = request.ReportedByWorkerId,
            AssignedInvestigatorId = request.AssignedInvestigatorId
        };

        var incidentId = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(
            nameof(GetByIdAsync),
            new { incidentId },
            ApiResponse<Guid>.Ok(incidentId, env.CorrelationId.ToString()));
    }

    /// <summary>Updates mutable fields of an existing Incident.</summary>
    /// <param name="incidentId">Incident to update.</param>
    /// <param name="request">Updated fields bound from the request body.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the updated Incident's identifier.</returns>
    /// <response code="200">Incident updated successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPut("{incidentId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateAsync(
        [FromRoute] Guid incidentId,
        [FromBody] UpdateIncidentRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new UpdateIncidentCommand
        {
            CommandId = env.CommandId,
            CorrelationId = env.CorrelationId,
            CausationId = env.CausationId,
            RequestedAtUtc = env.RequestedAtUtc,
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId,
            ActorId = env.ActorId,
            ETag = request.ETag,
            IncidentId = incidentId,
            Title = request.Title,
            Description = request.Description,
            Severity = request.Severity,
            Location = request.Location,
            AssignedInvestigatorId = request.AssignedInvestigatorId
        };

        var updatedId = await _sender.Send(command, cancellationToken);

        return Ok(ApiResponse<Guid>.Ok(updatedId, env.CorrelationId.ToString()));
    }

    /// <summary>Soft-deletes an Incident.</summary>
    /// <param name="incidentId">Incident to delete.</param>
    /// <param name="request">Deletion reason and concurrency token bound from the request body.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> confirming the deletion.</returns>
    /// <response code="200">Incident soft-deleted successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpDelete("{incidentId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteAsync(
        [FromRoute] Guid incidentId,
        [FromBody] DeleteIncidentRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new DeleteIncidentCommand
        {
            CommandId = env.CommandId,
            CorrelationId = env.CorrelationId,
            CausationId = env.CausationId,
            RequestedAtUtc = env.RequestedAtUtc,
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId,
            ActorId = env.ActorId,
            ETag = request.ETag,
            IncidentId = incidentId,
            DeletionReason = request.DeletionReason
        };

        await _sender.Send(command, cancellationToken);

        return Ok(ApiResponse<Guid>.Ok(incidentId, env.CorrelationId.ToString()));
    }

    /// <summary>Returns the full Incident projection for a given identifier.</summary>
    /// <param name="incidentId">Incident to retrieve.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the <see cref="IncidentDto"/>.</returns>
    /// <response code="200">Incident found.</response>
    /// <response code="404">Incident not found or soft-deleted.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("{incidentId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IncidentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IncidentDto>), StatusCodes.Status404NotFound)]
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
            return NotFound(ApiResponse<IncidentDto>.Fail("Incident not found.", env.CorrelationId.ToString()));

        return Ok(ApiResponse<IncidentDto>.Ok(incident, env.CorrelationId.ToString()));
    }

    /// <summary>Returns a paginated list of Incidents for the caller's tenant.</summary>
    /// <param name="request">Filter and paging parameters bound from the query string.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the paginated Incident summaries.</returns>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.Common.Results.PaginatedResult<IncidentSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListAsync(
        [FromQuery] ListIncidentsRequest request,
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

        return Ok(ApiResponse<PlantSafety.Application.Common.Results.PaginatedResult<IncidentSummaryDto>>.Ok(result, env.CorrelationId.ToString()));
    }

    /// <summary>Performs a full-text search across Incidents within the caller's tenant.</summary>
    /// <param name="request">Search term and paging parameters bound from the query string.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the paginated Incident summaries.</returns>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.Common.Results.PaginatedResult<IncidentSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SearchAsync(
        [FromQuery] SearchIncidentsRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new SearchIncidentsQuery
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
            SearchTerm = request.SearchTerm,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        var result = await _sender.Send(query, cancellationToken);

        return Ok(ApiResponse<PlantSafety.Application.Common.Results.PaginatedResult<IncidentSummaryDto>>.Ok(result, env.CorrelationId.ToString()));
    }

    /// <summary>Exports Incident records as a structured byte payload (CSV or JSON).</summary>
    /// <param name="request">Export filters bound from the query string.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the exported byte payload.</returns>
    /// <response code="200">Export generated successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("export")]
    [ProducesResponseType(typeof(ApiResponse<byte[]>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportAsync(
        [FromQuery] ExportIncidentsRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new ExportIncidentsQuery
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
            Format = request.Format,
            SiteId = request.SiteId,
            OccurredFromUtc = request.OccurredFromUtc,
            OccurredToUtc = request.OccurredToUtc,
            MaxRecords = request.MaxRecords
        };

        var payload = await _sender.Send(query, cancellationToken);

        return Ok(ApiResponse<byte[]>.Ok(payload, env.CorrelationId.ToString()));
    }
}
