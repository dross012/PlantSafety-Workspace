// ============================================================================
// File: SensorIntegrityChecksV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (Claude Code modular-creation-agent)
// Created: 2026-08-04
// Description: V1 MVC controller exposing the Sensor Integrity Monitoring subdomain of the
//              CyberPhysicalSecurity module.
// ============================================================================
using PlantSafety.Api.Common;
using PlantSafety.Api.Context;
using PlantSafety.Api.CyberPhysicalSecurity.Contracts.Requests;
using PlantSafety.Application.CyberPhysicalSecurity.SensorIntegrityCheck.Commands;
using PlantSafety.Application.CyberPhysicalSecurity.SensorIntegrityCheck.Queries;

namespace PlantSafety.Api.CyberPhysicalSecurity.Controllers.V1;

/// <summary>V1 controller for the Sensor Integrity Monitoring subdomain.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/cyberphysicalsecurity/sensor-integrity-checks")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("CyberPhysicalSecurity.SensorIntegrityChecks")]
public sealed class SensorIntegrityChecksV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="SensorIntegrityChecksV1Controller"/>.</summary>
    public SensorIntegrityChecksV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Records a completed sensor-integrity check, either manually or from the recurring integrity-check scheduling hook.</summary>
    /// <response code="200">Sensor-integrity check recorded successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="422">Request validation failed.</response>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RunAsync([FromBody] RunSensorIntegrityCheckRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RunSensorIntegrityCheckCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.RequestedAtUtc,
            env.TenantId, request.SiteId, request.OtAssetId, request.CheckType, request.Result,
            request.Details ?? string.Empty, env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns the sensor-integrity check history for a given OT asset, most recent first.</summary>
    /// <response code="200">History retrieved successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("assets/{otAssetId:guid}/history")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetHistoryAsync(Guid otAssetId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetSensorIntegrityHistoryQuery(env.TenantId, otAssetId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
