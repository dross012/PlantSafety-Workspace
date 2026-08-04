// ============================================================================
// File: ControlChangeEventsV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (Claude Code modular-creation-agent)
// Created: 2026-08-04
// Description: V1 MVC controller exposing the Anomalous Control-Change Detection subdomain of the
//              CyberPhysicalSecurity module. Backs the Control-Change Audit Feed dashboard.
// ============================================================================
using PlantSafety.Api.Common;
using PlantSafety.Api.Context;
using PlantSafety.Api.CyberPhysicalSecurity.Contracts.Requests;
using PlantSafety.Application.CyberPhysicalSecurity.ControlChangeEvent.Commands;
using PlantSafety.Application.CyberPhysicalSecurity.ControlChangeEvent.Queries;

namespace PlantSafety.Api.CyberPhysicalSecurity.Controllers.V1;

/// <summary>V1 controller for the Anomalous Control-Change Detection subdomain.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/cyberphysicalsecurity/control-change-events")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("CyberPhysicalSecurity.ControlChangeEvents")]
public sealed class ControlChangeEventsV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="ControlChangeEventsV1Controller"/>.</summary>
    public ControlChangeEventsV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Records a new OT control-system change observation, typically ingested from SCADA/HMI/ICS telemetry.</summary>
    /// <response code="200">Control-change event recorded successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="422">Request validation failed.</response>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RecordAsync([FromBody] RecordControlChangeEventRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RecordControlChangeEventCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.RequestedAtUtc,
            env.TenantId, request.SiteId, request.OtAssetId, request.ChangeDescription ?? string.Empty,
            request.ChangedParameter, request.OldValue ?? string.Empty, request.NewValue ?? string.Empty,
            request.DetectedAtUtc, request.ChangeSourceUserId, request.HasApprovedChangeRecord, env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Flags an existing control-change event as unauthorized, typically after a manual change-management review.</summary>
    /// <response code="200">Control-change event flagged successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">No control-change event exists with the given identity for this tenant.</response>
    [HttpPost("{controlChangeEventId:guid}/flag-unauthorized")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FlagUnauthorizedAsync(Guid controlChangeEventId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new FlagUnauthorizedControlChangeCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.RequestedAtUtc,
            env.TenantId, controlChangeEventId, env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Lists control-change events currently flagged unauthorized. Backs the Control-Change Audit Feed dashboard.</summary>
    /// <response code="200">List retrieved successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("unauthorized")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListUnauthorizedAsync(CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new ListUnauthorizedControlChangesQuery(env.TenantId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
