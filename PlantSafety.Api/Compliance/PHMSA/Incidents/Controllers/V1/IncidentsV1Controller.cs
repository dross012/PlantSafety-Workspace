// ============================================================================
// File: IncidentsV1Controller.cs
// Project: PlantSafety.Api
// Module: Compliance.PHMSA.Incidents
// Created: 2026-08-07
// Description: V1 controller exposing the Application-layer Compliance.PHMSA.Incidents module's
//              Commands and Queries over HTTP. Covers the full DOT Form 5800.1 incident-report
//              lifecycle: Create -> Submit -> BeginReview -> Close. Mirrors
//              PlantSafety.Api.Compliance.Permits.Controllers.V1.PermitsV1Controller.
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers().
// - Envelope fields (CommandId/CorrelationId/TenantId/ActorId etc.) are populated via the shared
//   RequestEnvelopeFactory rather than hand-rolled per action. ActorId always comes from the
//   authenticated caller (ICurrentUserService), never from client-supplied input.
// - All mutating actions use SafetyTierPolicies.Write; reads use SafetyTierPolicies.Read (mirrors
//   PermitsV1Controller's non-Approve-tier action policy choice -- this module has no separate
//   approval-gated transition).
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Compliance.PHMSA.Incidents.Contracts.Requests;
using PlantSafety.Application.Compliance.PHMSA.Incidents.Commands;
using PlantSafety.Application.Compliance.PHMSA.Incidents.Queries;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.Compliance.PHMSA.Incidents.Controllers.V1;

/// <summary>V1 controller for the Compliance.PHMSA.Incidents module's commands and queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/compliance/phmsa/incidents")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Compliance.PHMSA.Incidents")]
public sealed class IncidentsV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="IncidentsV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public IncidentsV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Files a new DOT Form 5800.1 hazardous materials incident report.</summary>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CreateIncidentReportResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CreateIncidentReportResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateIncidentReportRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateIncidentReportCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            IncidentType: request.IncidentType,
            MaterialInvolvedId: request.MaterialInvolvedId,
            MaterialInvolvedDescription: request.MaterialInvolvedDescription,
            Injuries: request.Injuries,
            FatalitiesInvolved: request.FatalitiesInvolved,
            PropertyDamageEstimateUsd: request.PropertyDamageEstimateUsd,
            Narrative: request.Narrative,
            IncidentDateUtc: request.IncidentDateUtc,
            Location: request.Location,
            ReportedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Submits a Draft incident report for review (Draft -&gt; Submitted).</summary>
    [HttpPost("{incidentReportId:guid}/submit")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<IncidentReportStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IncidentReportStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SubmitAsync(
        Guid incidentReportId,
        [FromBody] SubmitIncidentReportRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new SubmitIncidentReportCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            IncidentReportId: incidentReportId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Begins formal review of a submitted incident report (Submitted -&gt; UnderReview).</summary>
    [HttpPost("{incidentReportId:guid}/begin-review")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<IncidentReportStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IncidentReportStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> BeginReviewAsync(
        Guid incidentReportId,
        [FromBody] BeginIncidentReportReviewRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new BeginIncidentReportReviewCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            IncidentReportId: incidentReportId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Formally closes a reviewed incident report (UnderReview -&gt; Closed).</summary>
    [HttpPost("{incidentReportId:guid}/close")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<IncidentReportStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IncidentReportStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CloseAsync(
        Guid incidentReportId,
        [FromBody] CloseIncidentReportRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CloseIncidentReportCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            IncidentReportId: incidentReportId,
            PerformedByUserId: env.ActorId,
            ResolutionNotes: request?.ResolutionNotes)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single IncidentReport read-model record by identity, including its current ETag.</summary>
    [HttpGet("{incidentReportId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.Compliance.PHMSA.Incidents.Models.IncidentReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        Guid incidentReportId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetIncidentReportQuery(env.TenantId, incidentReportId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns IncidentReport read-model data for a tenant/site, optionally filtered by incident-date range and PHMSA reportability.</summary>
    [HttpGet]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<ListIncidentReportsResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ListIncidentReportsResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListAsync(
        [FromQuery] IncidentReportsListRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new ListIncidentReportsQuery(
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            FromUtc: request.FromUtc,
            ToUtc: request.ToUtc,
            IsReportableToPhmsa: request.IsReportableToPhmsa);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
