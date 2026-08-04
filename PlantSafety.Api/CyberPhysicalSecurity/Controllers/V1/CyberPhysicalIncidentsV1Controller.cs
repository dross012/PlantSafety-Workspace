// ============================================================================
// File: CyberPhysicalIncidentsV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (Claude Code modular-creation-agent)
// Created: 2026-08-04
// Description: V1 MVC controller exposing the Incident Response subdomain of the
//              CyberPhysicalSecurity module.
// ============================================================================
using PlantSafety.Api.Common;
using PlantSafety.Api.Context;
using PlantSafety.Api.CyberPhysicalSecurity.Contracts.Requests;
using PlantSafety.Application.CyberPhysicalSecurity.CyberPhysicalIncident.Commands;
using PlantSafety.Application.CyberPhysicalSecurity.CyberPhysicalIncident.Queries;

namespace PlantSafety.Api.CyberPhysicalSecurity.Controllers.V1;

/// <summary>V1 controller for the Incident Response subdomain.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/cyberphysicalsecurity/incidents")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("CyberPhysicalSecurity.Incidents")]
public sealed class CyberPhysicalIncidentsV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="CyberPhysicalIncidentsV1Controller"/>.</summary>
    public CyberPhysicalIncidentsV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Opens a new cyber-physical security incident.</summary>
    /// <response code="200">Incident opened successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="422">Request validation failed.</response>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> OpenAsync([FromBody] OpenCyberPhysicalIncidentRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new OpenCyberPhysicalIncidentCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.RequestedAtUtc,
            env.TenantId, request.SiteId, request.Title, request.Description ?? string.Empty, request.Severity,
            request.OwnerId, request.RelatedSabotageIndicatorIds, env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Formally closes an existing cyber-physical security incident, computing its response time.</summary>
    /// <response code="200">Incident closed successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">No incident exists with the given identity for this tenant.</response>
    /// <response code="422">Invalid lifecycle transition.</response>
    [HttpPost("{cyberPhysicalIncidentId:guid}/close")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CloseAsync(Guid cyberPhysicalIncidentId, [FromBody] CloseCyberPhysicalIncidentRequest? request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CloseCyberPhysicalIncidentCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.RequestedAtUtc,
            env.TenantId, cyberPhysicalIncidentId, env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single incident's current state.</summary>
    /// <response code="200">Incident retrieved successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">No incident exists with the given identity for this tenant.</response>
    [HttpGet("{cyberPhysicalIncidentId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(Guid cyberPhysicalIncidentId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetCyberPhysicalIncidentByIdQuery(env.TenantId, cyberPhysicalIncidentId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
