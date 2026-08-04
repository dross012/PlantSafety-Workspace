// ============================================================================
// File: PhysicalAccessControlV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-04
// Description: V1 controller exposing every PhysicalAccessControl module command
//              (AccessPoints, Credentials, AccessEvents, SafetyZones, AccessAnomalies,
//              RestrictedZonePolicies) and their read-model "Get"/"List" queries over HTTP.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/physicalaccesscontrol/{sub-component}/...
// - Commands/Queries return PlantSafety.Application.Common.Results.Result<T>,
//   mapped to ApiResponse<T> via PlantSafety.Api.Common.ResultResponseExtensions.ToActionResult.
// - Commands are positional records: (CommandId, CorrelationId, CausationId, TenantId, <Entity>Id,
//   ...domain fields..., PerformedByUserId). PerformedByUserId is always populated from the current
//   actor (env.ActorId); the aggregate id is always bound from the route, never the body.
// - IngestAccessEventAsync and DetectAccessAnomalyAsync are also invoked internally by the
//   Infrastructure.Integrations badge-system webhook/IoT ingestion pipeline and anomaly-detection
//   correlation job respectively, not solely via this HTTP surface.
//
// ============================================================================

using PlantSafety.Api.Common;
using PlantSafety.Api.Context;
using PlantSafety.Api.PhysicalAccessControl.Contracts.Requests;
using PlantSafety.Application.PhysicalAccessControl.Commands;
using PlantSafety.Application.PhysicalAccessControl.AccessPoints.Queries;
using PlantSafety.Application.PhysicalAccessControl.Credentials.Queries;
using PlantSafety.Application.PhysicalAccessControl.AccessEvents.Queries;
using PlantSafety.Application.PhysicalAccessControl.SafetyZones.Queries;
using PlantSafety.Application.PhysicalAccessControl.AccessAnomalies.Queries;

namespace PlantSafety.Api.PhysicalAccessControl.Controllers.V1;

/// <summary>V1 controller for the PhysicalAccessControl module's commands and read-model queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/physicalaccesscontrol")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("PhysicalAccessControl")]
public sealed class PhysicalAccessControlV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="PhysicalAccessControlV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public PhysicalAccessControlV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    // ══════════════════════════════════════════════════════════════════════
    // AccessPoints
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Registers a new physical access-control device.</summary>
    /// <response code="200">Access point registered.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("access-points")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<AccessPointActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AccessPointActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RegisterAccessPointAsync([FromBody] RegisterAccessPointRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RegisterAccessPointCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            request.SiteId, request.AccessPointCode, request.Description, request.DeviceType,
            request.SafetyZoneId, request.IsPerimeter, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Decommissions an AccessPoint record.</summary>
    /// <response code="200">Decommissioned.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("access-points/{accessPointId:guid}/decommission")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<AccessPointActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AccessPointActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<AccessPointActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DecommissionAccessPointAsync(Guid accessPointId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new DecommissionAccessPointCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            accessPointId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single AccessPoint by identity.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("access-points/{accessPointId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<GetAccessPointByIdResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAccessPointByIdAsync(Guid accessPointId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetAccessPointByIdQuery(env.TenantId, accessPointId);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // Credentials
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Issues a new badge/credential to a holder.</summary>
    /// <response code="200">Credential issued.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("credentials")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CredentialActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CredentialActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> IssueCredentialAsync([FromBody] IssueCredentialRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new IssueCredentialCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            request.SiteId, request.BadgeNumber, request.HolderUserId, request.HolderType,
            request.AccessLevel, request.ExpiresAtUtc, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Revokes a Credential record.</summary>
    /// <response code="200">Revoked.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("credentials/{credentialId:guid}/revoke")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CredentialActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CredentialActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<CredentialActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeCredentialAsync(
        Guid credentialId, [FromBody] RevokeCredentialRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RevokeCredentialCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            credentialId, request.Reason, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns the current lifecycle status of a single Credential.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("credentials/{credentialId:guid}/status")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<GetCredentialStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCredentialStatusAsync(Guid credentialId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetCredentialStatusQuery(env.TenantId, credentialId);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // AccessEvents
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Ingests a single raw physical-access event from an external badge system.</summary>
    /// <response code="200">Event ingested.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("access-events")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<AccessEventActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AccessEventActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> IngestAccessEventAsync([FromBody] IngestAccessEventRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new IngestAccessEventCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            request.SiteId, request.AccessPointId, request.CredentialId, request.SafetyZoneId,
            request.EventType, request.Direction, request.OccurredAtUtc, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Flags an AccessEvent record as anomalous.</summary>
    /// <response code="200">Flagged.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("access-events/{accessEventId:guid}/flag-anomaly")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<AccessEventActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AccessEventActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<AccessEventActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FlagAccessEventAnomalyAsync(
        Guid accessEventId, [FromBody] FlagAccessEventAnomalyRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new FlagAccessEventAnomalyCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            accessEventId, request.AnomalyType, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Lists AccessEvents for a given SafetyZone within a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("access-events")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<ListAccessEventsByZoneResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListAccessEventsByZoneAsync([FromQuery] ListAccessEventsByZoneRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new ListAccessEventsByZoneQuery(env.TenantId, request.SafetyZoneId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // SafetyZones
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Registers a new safety zone for access/occupancy correlation.</summary>
    /// <response code="200">Zone registered.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("safety-zones")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<SafetyZoneActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SafetyZoneActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RegisterSafetyZoneAsync([FromBody] RegisterSafetyZoneRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RegisterSafetyZoneCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            request.SiteId, request.ZoneCode, request.ZoneName, request.HazardLevel, request.MaxOccupancy, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Places a SafetyZone under a restricted-access policy.</summary>
    /// <response code="200">Restricted.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("safety-zones/{safetyZoneId:guid}/restrict")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<SafetyZoneActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SafetyZoneActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<SafetyZoneActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestrictSafetyZoneAsync(Guid safetyZoneId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RestrictSafetyZoneCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            safetyZoneId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns the current occupancy of a single SafetyZone, correlated from ingested AccessEvents.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("safety-zones/{safetyZoneId:guid}/occupancy")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<GetZoneOccupancyFromAccessEventsResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetZoneOccupancyFromAccessEventsAsync(Guid safetyZoneId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetZoneOccupancyFromAccessEventsQuery(env.TenantId, safetyZoneId);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // AccessAnomalies
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Records a newly detected physical-access anomaly.</summary>
    /// <response code="200">Anomaly recorded.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("access-anomalies")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<AccessAnomalyActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AccessAnomalyActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DetectAccessAnomalyAsync([FromBody] DetectAccessAnomalyRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new DetectAccessAnomalyCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            request.SiteId, request.AccessEventId, request.AccessPointId, request.CredentialId,
            request.SafetyZoneId, request.AnomalyType, request.SeverityScore, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Acknowledges an AccessAnomaly record.</summary>
    /// <response code="200">Acknowledged.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("access-anomalies/{accessAnomalyId:guid}/acknowledge")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<AccessAnomalyActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AccessAnomalyActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<AccessAnomalyActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AcknowledgeAccessAnomalyAsync(
        Guid accessAnomalyId, [FromBody] AcknowledgeAccessAnomalyRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new AcknowledgeAccessAnomalyCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            accessAnomalyId, request.AcknowledgedByUserId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Lists active (unresolved) anomalies for the Anomaly Feed dashboard.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("access-anomalies/active")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<ListActiveAnomaliesResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListActiveAnomaliesAsync([FromQuery] ListActiveAnomaliesRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new ListActiveAnomaliesQuery(env.TenantId, request.SiteId);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // RestrictedZonePolicies
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Defines a new restricted-zone access policy.</summary>
    /// <response code="200">Policy defined.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("restricted-zone-policies")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<RestrictedZonePolicyActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RestrictedZonePolicyActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DefineRestrictedZonePolicyAsync(
        [FromBody] DefineRestrictedZonePolicyRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new DefineRestrictedZonePolicyCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            request.SiteId, request.SafetyZoneId, request.PolicyName, request.AllowedAccessLevel,
            request.EnforceAfterHoursLockout, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Activates a RestrictedZonePolicy record for enforcement.</summary>
    /// <response code="200">Activated.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("restricted-zone-policies/{restrictedZonePolicyId:guid}/activate")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<RestrictedZonePolicyActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RestrictedZonePolicyActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<RestrictedZonePolicyActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateRestrictedZonePolicyAsync(Guid restrictedZonePolicyId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new ActivateRestrictedZonePolicyCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            restrictedZonePolicyId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }
}
