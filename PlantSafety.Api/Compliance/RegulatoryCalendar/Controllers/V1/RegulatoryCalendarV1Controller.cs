// ============================================================================
// File: RegulatoryCalendarV1Controller.cs
// Project: PlantSafety.Api
// Module: Compliance.RegulatoryCalendar
// Created: 2026-08-02
// Description: V1 controller exposing the Application-layer Compliance.RegulatoryCalendar
//              module's Commands and Queries over HTTP. Covers the full lifecycle:
//              Create -> Activate -> RecordCompliance -> Close -> Archive. Mirrors
//              PlantSafety.Api.Compliance.Permits.Controllers.V1.PermitsV1Controller.
//
// Route note: this controller's route ("api/v{version}/compliance/regulatorycalendar") does not
// collide with the pre-existing read-only category controllers under
// PlantSafety.Api/Compliance/RegulatoryCalendar/<Category>/<Type>/Controllers (Deadlines/
// Inspections/Renewals) -- those are routed via each type's own nested segment (confirmed by grep
// before finalizing this route).
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers().
// - Envelope fields (CommandId/CorrelationId/TenantId/ActorId etc.) are populated via the shared
//   RequestEnvelopeFactory rather than hand-rolled per action. ActorId always comes from the
//   authenticated caller (ICurrentUserService), never from client-supplied input.
// - All mutating actions use SafetyTierPolicies.Write; reads use SafetyTierPolicies.Read.
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Compliance.RegulatoryCalendar.Contracts.Requests;
using PlantSafety.Application.Compliance.RegulatoryCalendar.Commands;
using PlantSafety.Application.Compliance.RegulatoryCalendar.Queries;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.Compliance.RegulatoryCalendar.Controllers.V1;

/// <summary>V1 controller for the Compliance.RegulatoryCalendar module's commands and queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/compliance/regulatorycalendar")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Compliance.RegulatoryCalendar")]
public sealed class RegulatoryCalendarV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="RegulatoryCalendarV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public RegulatoryCalendarV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Submits a new RegulatoryCalendar obligation record.</summary>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CreateRegulatoryCalendarResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CreateRegulatoryCalendarResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateRegulatoryCalendarRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateRegulatoryCalendarCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            EventTitle: request.EventTitle,
            RegulatoryBody: request.RegulatoryBody,
            JurisdictionCode: request.JurisdictionCode,
            ComplianceCategory: request.ComplianceCategory,
            ResponsiblePartyId: request.ResponsiblePartyId,
            DueAtUtc: request.DueAtUtc,
            ReminderLeadDays: request.ReminderLeadDays,
            CreatedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Activates a RegulatoryCalendar obligation (Created -&gt; Active).</summary>
    [HttpPost("{regulatoryCalendarId:guid}/activate")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<RegulatoryCalendarStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RegulatoryCalendarStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ActivateAsync(
        Guid regulatoryCalendarId,
        [FromBody] ActivateRegulatoryCalendarRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ActivateRegulatoryCalendarCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            RegulatoryCalendarId: regulatoryCalendarId,
            ActivatedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records compliance with a RegulatoryCalendar obligation (Active -&gt; Completed).</summary>
    [HttpPost("{regulatoryCalendarId:guid}/record-compliance")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<RegulatoryCalendarStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RegulatoryCalendarStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RecordComplianceAsync(
        Guid regulatoryCalendarId,
        [FromBody] RecordRegulatoryCalendarComplianceRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RecordRegulatoryCalendarComplianceCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            RegulatoryCalendarId: regulatoryCalendarId,
            CompletedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Formally closes a completed RegulatoryCalendar obligation (Completed -&gt; Closed).</summary>
    [HttpPost("{regulatoryCalendarId:guid}/close")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<RegulatoryCalendarStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RegulatoryCalendarStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CloseAsync(
        Guid regulatoryCalendarId,
        [FromBody] CloseRegulatoryCalendarRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CloseRegulatoryCalendarCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            RegulatoryCalendarId: regulatoryCalendarId,
            ClosedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Archives a closed RegulatoryCalendar obligation for compliance retention (Closed -&gt; Archived).</summary>
    [HttpPost("{regulatoryCalendarId:guid}/archive")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<RegulatoryCalendarStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RegulatoryCalendarStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ArchiveAsync(
        Guid regulatoryCalendarId,
        [FromBody] ArchiveRegulatoryCalendarRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ArchiveRegulatoryCalendarCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            RegulatoryCalendarId: regulatoryCalendarId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single RegulatoryCalendar read-model record by identity, including its current ETag.</summary>
    [HttpGet("{regulatoryCalendarId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.Compliance.RegulatoryCalendar.Models.RegulatoryCalendarItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        Guid regulatoryCalendarId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetRegulatoryCalendarByIdQuery(env.TenantId, regulatoryCalendarId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns RegulatoryCalendar read-model data for a tenant/site within a date range.</summary>
    [HttpGet]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<RegulatoryCalendarQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RegulatoryCalendarQueryResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRegulatoryCalendarAsync(
        [FromQuery] RegulatoryCalendarListRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetRegulatoryCalendarQuery(
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            From: request.From ?? DateTime.UtcNow.AddDays(-90),
            To: request.To ?? DateTime.UtcNow.AddDays(90));

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
