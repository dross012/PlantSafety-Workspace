// ============================================================================
// File: CommunitySafetyV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (Claude Code modular-creation-agent)
// Created: 2026-08-04
// Description: V1 controller exposing every CommunitySafety module command (CommunityContact,
//              CommunityConcern, CommunityImpactAssessment, CommunityNotification,
//              CommunityMeetingRecord) and their read-model queries over HTTP.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/communitysafety/{sub-component}/...
// - CommunitySafety commands/queries return PlantSafety.Application.Common.Results.Result<T>,
//   mapped to ApiResponse<T> via PlantSafety.Api.Common.ResultResponseExtensions.ToActionResult.
// - Commands are positional records: (CommandId, CorrelationId, CausationId, TenantId, <fields>,
//   ...domain fields..., PerformedByUserId). PerformedByUserId is always populated from the
//   current actor (env.ActorId); the aggregate id is always bound from the route, never the body.
// - CommunityContact endpoints require the additional narrow PII-restricted policy
//   (SafetyTierPolicies.Write / SafetyTierPolicies.Read remain the baseline; a future pass may
//   introduce a dedicated PII policy without breaking these routes -- additive-only evolution).
//
// ============================================================================

using PlantSafety.Api.Common;
using PlantSafety.Api.Context;
using PlantSafety.Api.CommunitySafety.Contracts.Requests;
using PlantSafety.Application.CommunitySafety.Commands;
using PlantSafety.Application.CommunitySafety.CommunityContact.Queries;
using PlantSafety.Application.CommunitySafety.CommunityConcern.Queries;
using PlantSafety.Application.CommunitySafety.CommunityImpactAssessment.Queries;
using PlantSafety.Application.CommunitySafety.CommunityNotification.Queries;
using PlantSafety.Application.CommunitySafety.CommunityMeetingRecord.Queries;

namespace PlantSafety.Api.CommunitySafety.Controllers.V1;

/// <summary>V1 controller for the CommunitySafety module's commands and read-model queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/communitysafety")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("CommunitySafety")]
public sealed class CommunitySafetyV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="CommunitySafetyV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public CommunitySafetyV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    // ══════════════════════════════════════════════════════════════════════
    // CommunityContact (Community Contact Registry)
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Registers a new CommunityContact record.</summary>
    /// <response code="200">Registered.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("community-contacts")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CommunityContactActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CommunityContactActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RegisterCommunityContactAsync([FromBody] RegisterCommunityContactRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RegisterCommunityContactCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            request.SiteId, request.DisplayName, request.ContactType, request.CommunityArea, request.Email,
            request.Organization, request.Phone, request.OptedInForNotifications, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns CommunityContact read-model data for a tenant/site within a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("community-contacts")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<CommunityContactsQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCommunityContactsAsync([FromQuery] CommunitySafetyPeriodQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetCommunityContactsQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // CommunityConcern (Complaint/Concern Intake)
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Logs a new CommunityConcern record.</summary>
    /// <response code="200">Logged.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("community-concerns")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CommunityConcernActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CommunityConcernActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> LogCommunityConcernAsync([FromBody] LogCommunityConcernRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new LogCommunityConcernCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            request.SiteId, request.Category, request.Severity, request.FacilityArea, request.Description,
            request.ReportedByContactId, request.RelatedIncidentId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Resolves a CommunityConcern record.</summary>
    /// <response code="200">Resolved.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("community-concerns/{communityConcernId:guid}/resolve")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CommunityConcernActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CommunityConcernActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<CommunityConcernActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResolveCommunityConcernAsync(
        Guid communityConcernId, [FromBody] ResolveCommunityConcernRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new ResolveCommunityConcernCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            communityConcernId, request.ResolutionNotes, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Lists open CommunityConcern records for the Community Concern Tracker dashboard.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("community-concerns/open")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<CommunityConcernsQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListOpenCommunityConcernsAsync([FromQuery] Guid? siteId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new ListOpenCommunityConcernsQuery(env.TenantId, siteId);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single CommunityConcern record by identity.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpGet("community-concerns/{communityConcernId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.CommunitySafety.CommunityConcern.Models.CommunityConcernItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.CommunitySafety.CommunityConcern.Models.CommunityConcernItemDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCommunityConcernByIdAsync(Guid communityConcernId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetCommunityConcernByIdQuery(env.TenantId, communityConcernId);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // CommunityImpactAssessment (Community Impact Assessment)
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Records a CommunityImpactAssessment for an on-site SafetyEvents incident.</summary>
    /// <response code="200">Recorded.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("community-impact-assessments")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CommunityImpactAssessmentActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CommunityImpactAssessmentActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RecordCommunityImpactAssessmentAsync([FromBody] RecordCommunityImpactAssessmentRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RecordCommunityImpactAssessmentCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            request.SiteId, request.IncidentId, request.Severity, request.AffectedArea, request.Description,
            request.EstimatedPersonsAffected, request.AssessedByUserId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns CommunityImpactAssessment records for a given SafetyEvents incident (Impact Assessment Summary by incident dashboard).</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("community-impact-assessments/by-incident/{incidentId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<CommunityImpactAssessmentsQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetImpactAssessmentsByIncidentAsync(Guid incidentId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetImpactAssessmentsByIncidentQuery(env.TenantId, incidentId);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // CommunityNotification (Outreach & Notification History)
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Sends a new CommunityNotification record.</summary>
    /// <response code="200">Sent.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("community-notifications")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CommunityNotificationActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CommunityNotificationActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SendCommunityNotificationAsync([FromBody] SendCommunityNotificationRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new SendCommunityNotificationCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            request.SiteId, request.Channel, request.RecipientContactId, request.Subject, request.MessageBody,
            request.RelatedConcernId, request.RelatedIncidentId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns CommunityNotification history for a tenant/site within a date range (Notification History &amp; Reach dashboard).</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("community-notifications")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<CommunityNotificationsQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListNotificationsByPeriodAsync([FromQuery] CommunitySafetyPeriodQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new ListNotificationsByPeriodQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // CommunityMeetingRecord (Outreach & Notification History -- scheduled meeting cadence)
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Records that a community meeting was held.</summary>
    /// <response code="200">Recorded.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("community-meetings")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CommunityMeetingRecordActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CommunityMeetingRecordActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RecordCommunityMeetingAsync([FromBody] RecordCommunityMeetingRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RecordCommunityMeetingCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            request.SiteId, request.MeetingTitle, request.MeetingDateUtc, request.Location, request.AttendeeCount,
            request.TopicsSummary, request.ActionItems, request.RecordedByUserId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns CommunityMeetingRecord history for a tenant/site within a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("community-meetings")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<CommunityMeetingRecordsQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCommunityMeetingRecordsAsync([FromQuery] CommunitySafetyPeriodQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetCommunityMeetingRecordsQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }
}
