// ============================================================================
// File: SabotageIndicatorsV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (Claude Code modular-creation-agent)
// Created: 2026-08-04
// Description: V1 MVC controller exposing the Sabotage Indicator Correlation subdomain of the
//              CyberPhysicalSecurity module. Backs the Active Sabotage Indicator Board dashboard.
// ============================================================================
using PlantSafety.Api.Common;
using PlantSafety.Api.Context;
using PlantSafety.Api.CyberPhysicalSecurity.Contracts.Requests;
using PlantSafety.Application.CyberPhysicalSecurity.SabotageIndicator.Commands;
using PlantSafety.Application.CyberPhysicalSecurity.SabotageIndicator.Queries;

namespace PlantSafety.Api.CyberPhysicalSecurity.Controllers.V1;

/// <summary>V1 controller for the Sabotage Indicator Correlation subdomain.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/cyberphysicalsecurity/sabotage-indicators")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("CyberPhysicalSecurity.SabotageIndicators")]
public sealed class SabotageIndicatorsV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="SabotageIndicatorsV1Controller"/>.</summary>
    public SabotageIndicatorsV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Raises a new sabotage indicator. Correlated multi-source indicators auto-escalate per the module's risk hooks.</summary>
    /// <response code="200">Sabotage indicator raised successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="422">Request validation failed.</response>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RaiseAsync([FromBody] RaiseSabotageIndicatorRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RaiseSabotageIndicatorCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.RequestedAtUtc,
            env.TenantId, request.SiteId, request.Source, request.Description, request.Severity,
            request.SourceReferenceId, env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Lists currently active (Open/UnderInvestigation) sabotage indicators. Backs the Active Sabotage Indicator Board.</summary>
    /// <response code="200">List retrieved successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("active")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListActiveAsync(CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new ListActiveSabotageIndicatorsQuery(env.TenantId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
