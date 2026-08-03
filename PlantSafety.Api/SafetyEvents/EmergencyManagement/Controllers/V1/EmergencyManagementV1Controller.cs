// ============================================================================
// File: EmergencyManagementV1Controller.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: V1 MVC controller exposing GET endpoints for the EmergencyManagement read-model.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow /api/v1/safety-events/emergency-management/...
//
// Rationale:
// - Routes all reads through MediatR ISender to the Application layer. No domain logic or data access here.
//
// Future Extension Seams:
// - Add [OutputCache(PolicyName = ...)] attributes when tenant-scoped cache policies are wired in ServiceCollectionExtensions.
//
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Common;
using PlantSafety.Api.SafetyEvents.EmergencyManagement.Contracts.Requests;
using PlantSafety.Api.SafetyEvents.EmergencyManagement.Contracts.Responses;
using PlantSafety.Api.SafetyEvents.EmergencyManagement.Authorization;
using PlantSafety.Application.SafetyEvents.EmergencyManagement.Commands;
using PlantSafety.Application.SafetyEvents.EmergencyManagement.Queries;

namespace PlantSafety.Api.SafetyEvents.EmergencyManagement.Controllers.V1;

/// <summary>V1 controller for EmergencyManagement read and write endpoints.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/safety-events/emergency-management")]
[Authorize(Policy = EmergencyManagementAuthorizationPolicies.Read)]
[Tags("EmergencyManagement")]
public sealed class EmergencyManagementV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="EmergencyManagementV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public EmergencyManagementV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Declares a new emergency (Phase 4 full build-out lifecycle write path).</summary>
    [HttpPost]
    [Authorize(Policy = EmergencyManagementAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<DeclareEmergencyResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DeclareEmergencyResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeclareAsync(
        [FromBody] DeclareEmergencyRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new DeclareEmergencyCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            EmergencyType: request.EmergencyType,
            Severity: request.Severity,
            Location: request.Location,
            CasualtyCount: request.CasualtyCount,
            ResponseTeamNotified: request.ResponseTeamNotified,
            DeclaredByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records that the response team has begun responding to the declared emergency (Created -&gt; Active).</summary>
    [HttpPost("{emergencyManagementId:guid}/respond")]
    [Authorize(Policy = EmergencyManagementAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyManagementStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyManagementStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RespondToAsync(
        Guid emergencyManagementId,
        [FromBody] RespondToEmergencyRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RespondToEmergencyCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            EmergencyManagementId: emergencyManagementId,
            RespondedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records that an active emergency has been stabilized.</summary>
    [HttpPost("{emergencyManagementId:guid}/stabilize")]
    [Authorize(Policy = EmergencyManagementAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyManagementStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyManagementStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> StabilizeAsync(
        Guid emergencyManagementId,
        [FromBody] StabilizeEmergencyRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new StabilizeEmergencyCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            EmergencyManagementId: emergencyManagementId,
            StabilizedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Resolves an active emergency (Active -&gt; Completed).</summary>
    [HttpPost("{emergencyManagementId:guid}/resolve")]
    [Authorize(Policy = EmergencyManagementAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyManagementStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyManagementStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ResolveAsync(
        Guid emergencyManagementId,
        [FromBody] ResolveEmergencyRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ResolveEmergencyCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            EmergencyManagementId: emergencyManagementId,
            ResolvedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Closes a resolved emergency record (Completed -&gt; Closed).</summary>
    [HttpPost("{emergencyManagementId:guid}/close")]
    [Authorize(Policy = EmergencyManagementAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyManagementStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyManagementStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CloseAsync(
        Guid emergencyManagementId,
        [FromBody] CloseEmergencyRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CloseEmergencyCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            EmergencyManagementId: emergencyManagementId,
            ClosedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Archives a closed emergency record for compliance retention (Closed -&gt; Archived).</summary>
    [HttpPost("{emergencyManagementId:guid}/archive")]
    [Authorize(Policy = EmergencyManagementAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyManagementStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyManagementStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ArchiveAsync(
        Guid emergencyManagementId,
        [FromBody] ArchiveEmergencyRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ArchiveEmergencyCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            EmergencyManagementId: emergencyManagementId,
            ArchivedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single EmergencyManagement read-model record by identity, including its current ETag.</summary>
    [HttpGet("{emergencyManagementId:guid}")]
    [Authorize(Policy = EmergencyManagementAuthorizationPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.SafetyEvents.EmergencyManagement.Models.EmergencyManagementItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        Guid emergencyManagementId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetEmergencyManagementByIdQuery(env.TenantId, emergencyManagementId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns EmergencyManagement data for the resolved tenant.</summary>
    /// <param name="request">Query parameters bound from the query string.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the EmergencyManagement result set.</returns>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<EmergencyManagementResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAsync(
        [FromQuery] GetEmergencyManagementRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var correlationId = env.CorrelationId.ToString();

        var query  = new GetEmergencyManagementQuery(env.TenantId, request.SiteId, request.From, request.To ?? DateTime.UtcNow);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<EmergencyManagementResponse>.Fail(result.Error.Message, correlationId));

        var response = new EmergencyManagementResponse(
            result.Value.TenantId,
            result.Value.SiteId,
            result.Value.GeneratedAt,
            result.Value.Items);

        return Ok(ApiResponse<EmergencyManagementResponse>.Ok(response, correlationId));
    }
}