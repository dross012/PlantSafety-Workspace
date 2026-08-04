// ============================================================================
// File: PublicSafetyCommunicationV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-04
// Description: V1 controller exposing the Application-layer PublicSafetyCommunication module's
//              Commands and Queries over HTTP. Covers Agency Contact Directory registration,
//              Incident Notification Dispatch (dispatch/acknowledge/close), Public Alert
//              Broadcast, and Communication Log & After-Action recording/review.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/publicsafetycommunication/...
// - Envelope fields (CommandId/CorrelationId/TenantId/ActorId etc.) are populated via the
//   shared RequestEnvelopeFactory rather than hand-rolled per action. ActorId always comes
//   from the authenticated caller (ICurrentUserService), never from client-supplied input.
//
// Rationale:
// - Routes all reads/writes through MediatR ISender to the Application layer. No domain
//   logic or data access here.
//
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.PublicSafetyCommunication.Contracts.Requests;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.PublicSafetyCommunication.Commands;
using PlantSafety.Application.PublicSafetyCommunication.AgencyDirectory.Queries;
using PlantSafety.Application.PublicSafetyCommunication.IncidentNotificationDispatch.Queries;
using PlantSafety.Application.PublicSafetyCommunication.PublicAlertBroadcast.Queries;
using PlantSafety.Application.PublicSafetyCommunication.CommunicationLog.Queries;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.PublicSafetyCommunication.Controllers.V1;

/// <summary>V1 controller for the PublicSafetyCommunication module's commands and queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/publicsafetycommunication")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("PublicSafetyCommunication")]
public sealed class PublicSafetyCommunicationV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="PublicSafetyCommunicationV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public PublicSafetyCommunicationV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    // ── Agency Contact Directory ─────────────────────────────────────────────

    /// <summary>Registers a new external public-safety agency in the directory.</summary>
    [HttpPost("agencies")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PublicSafetyAgencyActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PublicSafetyAgencyActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RegisterPublicSafetyAgencyAsync(
        [FromBody] RegisterPublicSafetyAgencyRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RegisterPublicSafetyAgencyCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            AgencyName: request.AgencyName,
            AgencyType: request.AgencyType,
            JurisdictionDescription: request.JurisdictionDescription,
            PrimaryPhoneNumber: request.PrimaryPhoneNumber,
            PrimaryEmail: request.PrimaryEmail)
        {
            ActorId = env.ActorId
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns the PublicSafetyAgency directory for the Agency Notification Status Board dashboard.</summary>
    [HttpGet("agencies")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<ListPublicSafetyAgenciesQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ListPublicSafetyAgenciesQueryResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListPublicSafetyAgenciesAsync(
        [FromQuery] ListPublicSafetyAgenciesRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new ListPublicSafetyAgenciesQuery(
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            AgencyType: request.AgencyType,
            ActiveOnly: request.ActiveOnly);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    // ── Incident Notification Dispatch ───────────────────────────────────────

    /// <summary>Dispatches a new outbound incident notification to an external public-safety agency.</summary>
    [HttpPost("notifications")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<IncidentNotificationActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IncidentNotificationActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DispatchIncidentNotificationAsync(
        [FromBody] DispatchIncidentNotificationRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new DispatchIncidentNotificationCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            IncidentId: request.IncidentId,
            PublicSafetyAgencyId: request.PublicSafetyAgencyId,
            DeliveryChannelCode: request.DeliveryChannelCode,
            Message: request.Message)
        {
            ActorId = env.ActorId
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records the receiving agency's acknowledgement of a dispatched notification.</summary>
    [HttpPost("notifications/{incidentNotificationId:guid}/acknowledge")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<IncidentNotificationActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IncidentNotificationActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AcknowledgeIncidentNotificationAsync(
        Guid incidentNotificationId,
        [FromBody] AcknowledgeIncidentNotificationRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new AcknowledgeIncidentNotificationCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            IncidentNotificationId: incidentNotificationId,
            AcknowledgedByContactName: request.AcknowledgedByContactName)
        {
            ActorId = env.ActorId,
            ETag = request.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Closes an incident notification's dispatch lifecycle.</summary>
    [HttpPost("notifications/{incidentNotificationId:guid}/close")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<IncidentNotificationActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IncidentNotificationActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CloseIncidentNotificationAsync(
        Guid incidentNotificationId,
        [FromBody] CloseIncidentNotificationRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CloseIncidentNotificationCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            IncidentNotificationId: incidentNotificationId)
        {
            ActorId = env.ActorId,
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns IncidentNotification dispatch status for the notification dispatch latency and agency acknowledgement rate KPIs.</summary>
    [HttpGet("notifications")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<GetIncidentNotificationStatusQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<GetIncidentNotificationStatusQueryResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetIncidentNotificationStatusAsync(
        [FromQuery] IncidentNotificationStatusRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetIncidentNotificationStatusQuery(
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            IncidentId: request.IncidentId,
            From: request.From ?? DateTime.UtcNow.AddDays(-90),
            To: request.To ?? DateTime.UtcNow.AddDays(90));

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    // ── Public Alert Broadcast ────────────────────────────────────────────────

    /// <summary>Sends a new public alert broadcast to a public warning system (EAS/WEA/IPAWS, siren network, social media).</summary>
    [HttpPost("broadcasts")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<PublicAlertBroadcastActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PublicAlertBroadcastActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> BroadcastPublicAlertAsync(
        [FromBody] BroadcastPublicAlertRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new BroadcastPublicAlertCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            IncidentId: request.IncidentId,
            SeverityCode: request.SeverityCode,
            ChannelCode: request.ChannelCode,
            Headline: request.Headline,
            Message: request.Message,
            TargetArea: request.TargetArea)
        {
            ActorId = env.ActorId
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns active broadcasts for the Broadcast History dashboard and broadcast delivery success rate KPI.</summary>
    [HttpGet("broadcasts/active")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<ListActiveBroadcastsQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ListActiveBroadcastsQueryResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListActiveBroadcastsAsync(
        [FromQuery] ListActiveBroadcastsRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new ListActiveBroadcastsQuery(
            TenantId: env.TenantId,
            SiteId: request.SiteId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    // ── Communication Log & After-Action ─────────────────────────────────────

    /// <summary>Records a new, immutable communication log entry documenting a communication with an external stakeholder.</summary>
    [HttpPost("communication-log")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CommunicationLogEntryActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CommunicationLogEntryActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> LogCommunicationEntryAsync(
        [FromBody] LogCommunicationEntryRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new LogCommunicationEntryCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            IncidentId: request.IncidentId,
            RelatedIncidentNotificationId: request.RelatedIncidentNotificationId,
            RelatedPublicAlertBroadcastId: request.RelatedPublicAlertBroadcastId,
            Direction: request.Direction,
            Medium: request.Medium,
            CounterpartyName: request.CounterpartyName,
            Summary: request.Summary,
            OccurredAtUtc: request.OccurredAtUtc)
        {
            ActorId = env.ActorId
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns the full communication log for an incident, for the After-Action Communication Timeline dashboard.</summary>
    [HttpGet("communication-log/{incidentId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<GetCommunicationLogByIncidentQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<GetCommunicationLogByIncidentQueryResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCommunicationLogByIncidentAsync(
        Guid incidentId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetCommunicationLogByIncidentQuery(
            TenantId: env.TenantId,
            IncidentId: incidentId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
