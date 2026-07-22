// ============================================================================
// File: AlertsV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-21
// Description: V1 MVC controller exposing the Application-layer Alerts Commands and Queries
//              over HTTP.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/alerts/...
// - Envelope fields (CommandId/CorrelationId/TraceId/etc.) are populated via the shared
//   RequestEnvelopeFactory rather than hand-rolled per action.
// - Alerts.Commands implement ICommand<Result<T>> (full envelope incl. TenantId/ActorId).
//   Alerts.Queries implement plain IRequest<Result<T>> (TenantId only, no full query
//   envelope) - TenantId is still sourced from RequestEnvelopeFactory so the caller cannot
//   spoof another tenant's data via the query string.
// - 7 of the 8 mutation commands (Acknowledge/Close/Dispatch/Escalate/RecordDelivery/
//   RecordResponseAction/Resolve) carry a placeholder TenantId (defaults to Guid.Empty on the
//   record) since no natural tenant field exists on those aggregate operations yet; this
//   controller always sets TenantId = env.TenantId explicitly regardless, so behavior is
//   correct today even though the placeholder exists at the record-default level.
// - ActorId is a read-only computed property on every Alerts command, mapped to a
//   domain-specific "by user" field (e.g. AcknowledgedByUserId). Since the computed property
//   itself cannot be set, this controller sets the underlying field from env.ActorId instead.
// - Assumption: for EscalateAlertCommand, when the caller indicates WasAutoEscalated = true,
//   EscalatedByUserId is left null (matching the domain's "no human actor" semantics for
//   automatic escalation) rather than being forced to env.ActorId.
//
// Rationale:
// - Routes all reads/writes through MediatR ISender to the Application layer. No domain
//   logic or data access here.
//
// ============================================================================

using PlantSafety.Api.Alerts.Contracts.Requests;
using PlantSafety.Api.Context;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Alerts.Commands;
using PlantSafety.Application.Alerts.Models;
using PlantSafety.Application.Alerts.Queries;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.Alerts.Controllers.V1;

/// <summary>V1 controller for Alert lifecycle commands and queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/alerts")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Alerts")]
public sealed class AlertsV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="AlertsV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public AlertsV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Triggers a new Alert.</summary>
    /// <param name="request">Alert fields bound from the request body.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the <see cref="TriggerAlertResult"/>.</returns>
    /// <response code="200">Alert triggered successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TriggerAlertResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> TriggerAsync(
        [FromBody] TriggerAlertRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new TriggerAlertCommand(
            env.CorrelationId,
            env.CausationId,
            env.CommandId,
            env.RequestedAtUtc,
            env.ApiVersionToken,
            env.TenantId,
            request.SiteId,
            request.CategoryCode,
            request.SourceTypeCode,
            request.Title,
            request.LocationZoneName,
            request.Latitude,
            request.Longitude,
            request.SeverityCode,
            env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Acknowledges a triggered or escalated Alert.</summary>
    /// <param name="alertId">Alert to acknowledge.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the <see cref="AcknowledgeAlertResult"/>.</returns>
    /// <response code="200">Alert acknowledged successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("{alertId:guid}/acknowledge")]
    [ProducesResponseType(typeof(ApiResponse<AcknowledgeAlertResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AcknowledgeAsync(
        Guid alertId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new AcknowledgeAlertCommand(
            env.CorrelationId,
            env.CausationId,
            env.CommandId,
            env.RequestedAtUtc,
            alertId,
            env.ActorId)
        {
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records that a response has been dispatched for an acknowledged Alert.</summary>
    /// <param name="alertId">Alert to record a dispatch for.</param>
    /// <param name="request">Dispatch fields bound from the request body.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the <see cref="DispatchAlertResult"/>.</returns>
    /// <response code="200">Alert dispatch recorded successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("{alertId:guid}/dispatch")]
    [ProducesResponseType(typeof(ApiResponse<DispatchAlertResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DispatchAsync(
        Guid alertId,
        [FromBody] DispatchAlertRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new DispatchAlertCommand(
            env.CorrelationId,
            env.CausationId,
            env.RequestedAtUtc,
            alertId,
            request.DispatchedTo,
            env.ActorId)
        {
            CommandId = env.CommandId,
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Escalates an Alert due to lack of timely acknowledgement/response, or by manual override.</summary>
    /// <param name="alertId">Alert to escalate.</param>
    /// <param name="request">Escalation fields bound from the request body.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the <see cref="EscalateAlertResult"/>.</returns>
    /// <response code="200">Alert escalated successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("{alertId:guid}/escalate")]
    [ProducesResponseType(typeof(ApiResponse<EscalateAlertResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> EscalateAsync(
        Guid alertId,
        [FromBody] EscalateAlertRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new EscalateAlertCommand(
            env.CorrelationId,
            env.CausationId,
            env.RequestedAtUtc,
            alertId,
            request.EscalationReason,
            request.WasAutoEscalated,
            request.WasAutoEscalated ? null : env.ActorId)
        {
            CommandId = env.CommandId,
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Resolves the underlying hazard/condition behind an Alert.</summary>
    /// <param name="alertId">Alert to resolve.</param>
    /// <param name="request">Resolution fields bound from the request body.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the <see cref="ResolveAlertResult"/>.</returns>
    /// <response code="200">Alert resolved successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("{alertId:guid}/resolve")]
    [ProducesResponseType(typeof(ApiResponse<ResolveAlertResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ResolveAsync(
        Guid alertId,
        [FromBody] ResolveAlertRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ResolveAlertCommand(
            env.CorrelationId,
            env.CausationId,
            env.RequestedAtUtc,
            alertId,
            request.ResponseActionsTaken,
            env.ActorId)
        {
            CommandId = env.CommandId,
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Closes a resolved Alert, recording close-out verification.</summary>
    /// <param name="alertId">Alert to close.</param>
    /// <param name="request">Close-out fields bound from the request body.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the <see cref="CloseAlertResult"/>.</returns>
    /// <response code="200">Alert closed successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("{alertId:guid}/close")]
    [ProducesResponseType(typeof(ApiResponse<CloseAlertResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CloseAsync(
        Guid alertId,
        [FromBody] CloseAlertRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CloseAlertCommand(
            env.CorrelationId,
            env.CausationId,
            env.RequestedAtUtc,
            alertId,
            request.CloseOutVerification,
            env.ActorId)
        {
            CommandId = env.CommandId,
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records a single multi-channel delivery attempt for an Alert.</summary>
    /// <param name="alertId">Alert the delivery attempt belongs to.</param>
    /// <param name="request">Delivery fields bound from the request body.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the <see cref="RecordAlertDeliveryResult"/>.</returns>
    /// <response code="200">Delivery attempt recorded successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("{alertId:guid}/delivery")]
    [ProducesResponseType(typeof(ApiResponse<RecordAlertDeliveryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RecordDeliveryAsync(
        Guid alertId,
        [FromBody] RecordAlertDeliveryRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RecordAlertDeliveryCommand(
            env.CorrelationId,
            env.CausationId,
            env.RequestedAtUtc,
            alertId,
            request.ChannelCode,
            request.Succeeded,
            request.FailureReason,
            env.ActorId)
        {
            CommandId = env.CommandId,
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records a single response action taken against an Alert.</summary>
    /// <param name="alertId">Alert the response action was taken against.</param>
    /// <param name="request">Response action fields bound from the request body.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the <see cref="RecordAlertResponseActionResult"/>.</returns>
    /// <response code="200">Response action recorded successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("{alertId:guid}/response-action")]
    [ProducesResponseType(typeof(ApiResponse<RecordAlertResponseActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RecordResponseActionAsync(
        Guid alertId,
        [FromBody] RecordAlertResponseActionRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RecordAlertResponseActionCommand(
            env.CorrelationId,
            env.CausationId,
            env.RequestedAtUtc,
            alertId,
            request.ActionDescription,
            request.Notes,
            env.ActorId)
        {
            CommandId = env.CommandId,
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns all Alerts that have not yet been closed.</summary>
    /// <param name="siteId">Optional site filter.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the <see cref="GetActiveAlertsResult"/>.</returns>
    /// <response code="200">Active alerts returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<GetActiveAlertsResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetActiveAsync(
        [FromQuery] Guid? siteId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetActiveAlertsQuery(env.TenantId, siteId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single Alert by identifier.</summary>
    /// <param name="alertId">Alert to retrieve.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the <see cref="AlertDto"/>.</returns>
    /// <response code="200">Alert found.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Alert not found.</response>
    [HttpGet("{alertId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AlertDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<AlertDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(
        Guid alertId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetAlertByIdQuery(env.TenantId, alertId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns the multi-channel delivery attempts recorded for a single Alert.</summary>
    /// <param name="alertId">Alert whose delivery status is requested.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the delivery record list.</returns>
    /// <response code="200">Delivery status returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("{alertId:guid}/delivery")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AlertDeliveryRecordDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDeliveryStatusAsync(
        Guid alertId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetAlertDeliveryStatusQuery(alertId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns Alert logging/reporting history within a UTC date range.</summary>
    /// <param name="request">Filters and paging bound from the query string.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the paginated result set.</returns>
    /// <response code="200">Alert history returned successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("history")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<AlertDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetHistoryAsync(
        [FromQuery] ListAlertHistoryRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope(cachePreference: CachePreference.None);

        var query = new GetAlertHistoryQuery(
            env.TenantId,
            request.SiteId,
            request.From,
            request.To,
            request.PageNumber,
            request.PageSize);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns Alerts within a given catalog category.</summary>
    /// <param name="categoryCode">Catalog category code (e.g. "LifeSafetyEmergency").</param>
    /// <param name="siteId">Optional site filter.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the <see cref="GetAlertsByCategoryResult"/>.</returns>
    /// <response code="200">Alerts returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("category/{categoryCode}")]
    [ProducesResponseType(typeof(ApiResponse<GetAlertsByCategoryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByCategoryAsync(
        string categoryCode,
        [FromQuery] Guid? siteId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetAlertsByCategoryQuery(env.TenantId, siteId, categoryCode);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

}
