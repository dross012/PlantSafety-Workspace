// ============================================================================
// File: AccidentManagementV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (Claude Code modular-creation-agent)
// Created: 2026-07-21
// Description: V1 MVC controller exposing every AccidentManagement module command (report +
//              6 investigation-workflow commands) over HTTP.
// ============================================================================
//
// Onboarding Notes:
// - Envelope fields are populated via RequestEnvelopeFactory, mirroring the canonical
//   Features/Compliance/Permits pattern. Commands are dispatched through MediatR ISender.
// - AccidentInvestigationCommands.cs commands implement MediatR IRequest<Result<T>> directly
//   (not the ICommand<T> marker) and therefore carry no ApiVersionToken/ActorId fields; the
//   "*By" field on each command IS the acting user and is populated from the envelope ActorId.
//
// ============================================================================

using PlantSafety.Api.AccidentManagement.Contracts.Requests;
using PlantSafety.Api.Common;
using PlantSafety.Api.Context;
using PlantSafety.Application.AccidentManagement.Commands;

namespace PlantSafety.Api.AccidentManagement.Controllers.V1;

/// <summary>V1 controller for AccidentManagement report + investigation-workflow commands.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/accidentmanagement")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("AccidentManagement")]
public sealed class AccidentManagementV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="AccidentManagementV1Controller"/>.</summary>
    public AccidentManagementV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Reports a new Accident in Reported status.</summary>
    /// <response code="200">Accident reported successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="422">Request validation failed.</response>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CreateAccidentResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<CreateAccidentResult>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateAccidentRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = CreateAccidentCommand.FromDto(
            tenantId: env.TenantId,
            siteId: request.SiteId,
            accidentTitle: request.AccidentTitle,
            location: request.Location,
            severity: request.Severity,
            injuredPersonCount: request.InjuredPersonCount,
            occurredAtUtc: request.OccurredAtUtc,
            category: request.Category,
            ownerId: request.OwnerId,
            reviewRequired: request.ReviewRequired,
            correlationId: env.CorrelationId,
            causationId: env.CausationId,
            apiVersionToken: env.ApiVersionToken) with { ActorId = env.ActorId };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Assigns an investigator to a Reported accident.</summary>
    /// <response code="200">Investigator assigned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="422">Request or state transition invalid.</response>
    [HttpPost("{accidentId:guid}/assign-investigator")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<AccidentStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<AccidentStatusResult>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AssignInvestigatorAsync(Guid accidentId, [FromBody] AssignAccidentInvestigatorRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new AssignAccidentInvestigatorCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.RequestedAtUtc,
            env.TenantId, accidentId, request.InvestigatorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records the outcome of Root Cause Analysis (RCA/FMEA) for an accident under investigation.</summary>
    /// <response code="200">Root cause recorded successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="422">Request or state transition invalid.</response>
    [HttpPost("{accidentId:guid}/root-cause")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<AccidentStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<AccidentStatusResult>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RecordRootCauseAsync(Guid accidentId, [FromBody] RecordAccidentRootCauseRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RecordAccidentRootCauseCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.RequestedAtUtc,
            env.TenantId, accidentId, request.RootCauseCategory, request.Notes, env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Creates a new CAPA corrective action against an accident investigation.</summary>
    /// <response code="200">Corrective action created successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="422">Request or state transition invalid.</response>
    [HttpPost("{accidentId:guid}/corrective-actions")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CreateAccidentCorrectiveActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<CreateAccidentCorrectiveActionResult>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateCorrectiveActionAsync(Guid accidentId, [FromBody] CreateAccidentCorrectiveActionRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateAccidentCorrectiveActionCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.RequestedAtUtc,
            env.TenantId, accidentId, request.Category, request.Description, request.ResponsiblePersonId, request.DueAtUtc, env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Marks a tracked corrective action Completed with evidence.</summary>
    /// <response code="200">Corrective action completed successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="422">Request or state transition invalid.</response>
    [HttpPost("{accidentId:guid}/corrective-actions/{correctiveActionId:guid}/complete")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<AccidentStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<AccidentStatusResult>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CompleteCorrectiveActionAsync(Guid accidentId, Guid correctiveActionId, [FromBody] CompleteAccidentCorrectiveActionRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CompleteAccidentCorrectiveActionCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.RequestedAtUtc,
            env.TenantId, accidentId, correctiveActionId, request.CompletionEvidence, env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records completion of required regulatory documentation (e.g. OSHA 300/300A/301).</summary>
    /// <response code="200">Regulatory report filed successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="422">Request or state transition invalid.</response>
    [HttpPost("{accidentId:guid}/regulatory-report")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<AccidentStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<AccidentStatusResult>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> FileRegulatoryReportAsync(Guid accidentId, [FromBody] FileAccidentRegulatoryReportRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new FileAccidentRegulatoryReportCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.RequestedAtUtc,
            env.TenantId, accidentId, request.RegulatoryAgency, request.ReferenceNumber, env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Closes an accident investigation once all mandatory prerequisites are satisfied.</summary>
    /// <response code="200">Investigation closed successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="422">Request or state transition invalid.</response>
    [HttpPost("{accidentId:guid}/close")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<AccidentStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<AccidentStatusResult>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CloseInvestigationAsync(Guid accidentId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CloseAccidentInvestigationCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.RequestedAtUtc,
            env.TenantId, accidentId, env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
