// ============================================================================
// File: SustainabilityV1Controller.cs
// Project: PlantSafety.Api
// Module: Sustainability
// Created: 2026-08-02
// Description: V1 controller exposing the Application-layer Sustainability module's Commands and
//              Queries over HTTP. Covers the full lifecycle: Create -> Activate -> Complete ->
//              Close -> Archive. Mirrors
//              PlantSafety.Api.Compliance.Auditing.Controllers.V1.AuditingV1Controller.
//
// Route note: this controller's route ("api/v{version}/sustainability/initiatives") does not
// collide with the pre-existing, unrelated Features-vertical metric-tracking controllers under
// PlantSafety.Api/Features/Sustainability.
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers().
// - Envelope fields are populated via the shared RequestEnvelopeFactory. ActorId always comes
//   from the authenticated caller (ICurrentUserService), never from client-supplied input.
// - All mutating actions use SafetyTierPolicies.Write; reads use SafetyTierPolicies.Read.
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Sustainability.Contracts.Requests;
using PlantSafety.Application.Sustainability.Commands;
using PlantSafety.Application.Sustainability.Queries;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.Sustainability.Controllers.V1;

/// <summary>V1 controller for the Sustainability module's commands and queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/sustainability/initiatives")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Sustainability")]
public sealed class SustainabilityV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="SustainabilityV1Controller"/>.</summary>
    public SustainabilityV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Submits a new Sustainability initiative record.</summary>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CreateSustainabilityResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CreateSustainabilityResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateSustainabilityRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateSustainabilityCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            InitiativeName: request.InitiativeName,
            Category: request.Category,
            TargetValue: request.TargetValue,
            OwnerUserId: request.OwnerUserId,
            RequesterUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Activates a sustainability initiative (Created -&gt; Active).</summary>
    [HttpPost("{sustainabilityId:guid}/activate")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<SustainabilityStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SustainabilityStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ActivateAsync(
        Guid sustainabilityId,
        [FromBody] ActivateSustainabilityRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ActivateSustainabilityCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            SustainabilityId: sustainabilityId,
            ActivatedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Completes an active sustainability initiative (Active -&gt; Completed).</summary>
    [HttpPost("{sustainabilityId:guid}/complete")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<SustainabilityStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SustainabilityStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CompleteAsync(
        Guid sustainabilityId,
        [FromBody] CompleteSustainabilityRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CompleteSustainabilityCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            SustainabilityId: sustainabilityId,
            CompletedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Formally closes a completed sustainability initiative (Completed -&gt; Closed).</summary>
    [HttpPost("{sustainabilityId:guid}/close")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<SustainabilityStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SustainabilityStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CloseAsync(
        Guid sustainabilityId,
        [FromBody] CloseSustainabilityRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CloseSustainabilityCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            SustainabilityId: sustainabilityId,
            ClosedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Archives a closed sustainability initiative for compliance retention (Closed -&gt; Archived).</summary>
    [HttpPost("{sustainabilityId:guid}/archive")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<SustainabilityStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SustainabilityStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ArchiveAsync(
        Guid sustainabilityId,
        [FromBody] ArchiveSustainabilityRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ArchiveSustainabilityCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            SustainabilityId: sustainabilityId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single Sustainability read-model record by identity, including its current ETag.</summary>
    [HttpGet("{sustainabilityId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.Sustainability.Models.SustainabilityItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        Guid sustainabilityId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetSustainabilityByIdQuery(env.TenantId, sustainabilityId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
