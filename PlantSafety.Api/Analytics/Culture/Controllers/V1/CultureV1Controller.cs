// ============================================================================
// File: CultureV1Controller.cs
// Project: PlantSafety.Api
// Module: Analytics.Culture
// Created: 2026-08-02
// Description: V1 controller exposing the Application-layer Analytics.Culture module's root
//              aggregate commands and queries over HTTP. Covers the full survey-period lifecycle:
//              Create -> Review -> Publish -> Close -> Archive. Mirrors
//              PlantSafety.Api.Compliance.Permits.Controllers.V1.PermitsV1Controller.
//
// Route note: this controller's route ("api/v{version}/analytics/culture") does not collide with
// the pre-existing read-only Culture sub-feature controllers under
// PlantSafety.Api/Analytics/Culture/<SubFeature>/Controllers (Behavior, CultureIndex,
// EngagementScoring, TrendModeling), which are routed under their own type-specific segments.
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers().
// - Envelope fields (CommandId/CorrelationId/TenantId/ActorId etc.) are populated via the shared
//   RequestEnvelopeFactory rather than hand-rolled per action.
// - CreateAsync dispatches the pre-existing CreateCultureCommand, which is a fire-and-forget
//   IRequest (no Result<T>) per its established canonical shape -- unlike the lifecycle-transition
//   actions below, which use the full ICommand<Result<T>>+IConcurrencyAwareCommand envelope.
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Analytics.Culture.Contracts.Requests;
using PlantSafety.Application.Analytics.Culture.Commands;
using PlantSafety.Application.Analytics.Culture.Queries;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.Analytics.Culture.Controllers.V1;

/// <summary>V1 controller for the Analytics.Culture module's root aggregate commands and queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/analytics/culture")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Analytics.Culture")]
public sealed class CultureV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="CultureV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public CultureV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Submits a new Culture survey period.</summary>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateCultureRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var actorId = UserId.FromGuid(env.ActorId);

        var command = new CreateCultureCommand(
            TenantId: TenantId.FromGuid(env.TenantId),
            UserId: actorId,
            CorrelationId: new CorrelationId(env.CorrelationId),
            CausationId: new CausationId(env.CausationId ?? Guid.NewGuid()),
            Actor: PerformedByActor.User(actorId));

        await _sender.Send(command, cancellationToken);

        return Ok();
    }

    /// <summary>Reviews a submitted Culture survey period (Created -&gt; Active).</summary>
    [HttpPost("{cultureId:guid}/review")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<CultureStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CultureStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ReviewAsync(
        Guid cultureId,
        [FromBody] ReviewCultureRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ReviewCultureCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            CultureId: cultureId,
            ReviewedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Publishes a reviewed Culture survey period's results (Active -&gt; Completed).</summary>
    [HttpPost("{cultureId:guid}/publish")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CultureStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CultureStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> PublishAsync(
        Guid cultureId,
        [FromBody] PublishCultureRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new PublishCultureCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            CultureId: cultureId,
            PublishedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Formally closes a published Culture survey period (Completed -&gt; Closed).</summary>
    [HttpPost("{cultureId:guid}/close")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CultureStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CultureStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CloseAsync(
        Guid cultureId,
        [FromBody] CloseCultureRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CloseCultureCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            CultureId: cultureId,
            ClosedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Archives a closed Culture survey period for long-term retention (Closed -&gt; Archived).</summary>
    [HttpPost("{cultureId:guid}/archive")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CultureStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CultureStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ArchiveAsync(
        Guid cultureId,
        [FromBody] ArchiveCultureRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ArchiveCultureCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            CultureId: cultureId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single Culture read-model record by identity, including its current ETag.</summary>
    [HttpGet("{cultureId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.Analytics.Culture.Models.CultureItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        Guid cultureId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetCultureByIdQuery(env.TenantId, cultureId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
