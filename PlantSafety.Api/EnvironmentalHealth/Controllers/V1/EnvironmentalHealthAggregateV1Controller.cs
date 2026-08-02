// ============================================================================
// File: EnvironmentalHealthAggregateV1Controller.cs
// Project: PlantSafety.Api
// Module: EnvironmentalHealth
// Created: 2026-08-02
// Description: V1 controller exposing the Application-layer EnvironmentalHealth aggregate-root
//              module's Commands and Queries over HTTP. Covers the full lifecycle:
//              Create -> Activate -> Complete -> Close -> Archive. Named distinctly
//              ("EnvironmentalHealthAggregateV1Controller", not "EnvironmentalHealthV1Controller")
//              and routed under "environmental-health/records" to avoid any route collision
//              with the pre-existing, unrelated, LIVE Features.EnvironmentalHealth "reading"
//              vertical's own controller(s). Mirrors
//              PlantSafety.Api.Compliance.Auditing.Controllers.V1.AuditingV1Controller.
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers().
// - Envelope fields are populated via the shared RequestEnvelopeFactory. ActorId always comes
//   from the authenticated caller (ICurrentUserService), never from client-supplied input.
// - All mutating actions use SafetyTierPolicies.Write; reads use SafetyTierPolicies.Read.
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.EnvironmentalHealth.Contracts.Requests;
using PlantSafety.Application.EnvironmentalHealth.Commands;
using PlantSafety.Application.EnvironmentalHealth.Queries;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.EnvironmentalHealth.Controllers.V1;

/// <summary>V1 controller for the EnvironmentalHealth aggregate-root module's commands and queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/environmental-health/records")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("EnvironmentalHealth")]
public sealed class EnvironmentalHealthAggregateV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="EnvironmentalHealthAggregateV1Controller"/>.</summary>
    public EnvironmentalHealthAggregateV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Submits a new EnvironmentalHealth record.</summary>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CreateEnvironmentalHealthResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CreateEnvironmentalHealthResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateEnvironmentalHealthRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateEnvironmentalHealthCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            MonitoringType: request.MonitoringType,
            MeasuredValue: request.MeasuredValue,
            Unit: request.Unit,
            Threshold: request.Threshold,
            ThresholdExceeded: request.ThresholdExceeded,
            EvaluatedStatus: request.EvaluatedStatus,
            RequesterUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Activates an EnvironmentalHealth record (Created -&gt; Active).</summary>
    [HttpPost("{environmentalHealthId:guid}/activate")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<EnvironmentalHealthStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EnvironmentalHealthStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ActivateAsync(
        Guid environmentalHealthId,
        [FromBody] ActivateEnvironmentalHealthRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ActivateEnvironmentalHealthCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            EnvironmentalHealthId: environmentalHealthId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Completes an EnvironmentalHealth record (Active -&gt; Completed).</summary>
    [HttpPost("{environmentalHealthId:guid}/complete")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<EnvironmentalHealthStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EnvironmentalHealthStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CompleteAsync(
        Guid environmentalHealthId,
        [FromBody] CompleteEnvironmentalHealthRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CompleteEnvironmentalHealthCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            EnvironmentalHealthId: environmentalHealthId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Closes an EnvironmentalHealth record (Completed -&gt; Closed).</summary>
    [HttpPost("{environmentalHealthId:guid}/close")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<EnvironmentalHealthStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EnvironmentalHealthStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CloseAsync(
        Guid environmentalHealthId,
        [FromBody] CloseEnvironmentalHealthRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CloseEnvironmentalHealthCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            EnvironmentalHealthId: environmentalHealthId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Archives a closed EnvironmentalHealth record for long-term retention (Closed -&gt; Archived).</summary>
    [HttpPost("{environmentalHealthId:guid}/archive")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<EnvironmentalHealthStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EnvironmentalHealthStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ArchiveAsync(
        Guid environmentalHealthId,
        [FromBody] ArchiveEnvironmentalHealthRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ArchiveEnvironmentalHealthCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            EnvironmentalHealthId: environmentalHealthId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single EnvironmentalHealth read-model record by identity, including its current ETag.</summary>
    [HttpGet("{environmentalHealthId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.EnvironmentalHealth.Models.EnvironmentalHealthItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        Guid environmentalHealthId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetEnvironmentalHealthByIdQuery(env.TenantId, environmentalHealthId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
