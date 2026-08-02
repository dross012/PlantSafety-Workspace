// ============================================================================
// File: EngagementRecordsV1Controller.cs
// Project: PlantSafety.Api
// Module: Analytics.Engagement
// Created: 2026-08-02
// Description: V1 controller exposing the Application-layer Analytics.Engagement module's
//              lifecycle Commands and Queries over HTTP. Covers the full worker-engagement
//              lifecycle: Create -> Review -> RecordTouchpoint -> Complete -> Close -> Archive.
//              Mirrors PlantSafety.Api.Compliance.Permits.Controllers.V1.PermitsV1Controller.
//
// Route note: this controller's route ("api/v{version}/analytics/engagement/records") does not
// collide with the pre-existing PlantSafety.Api.Analytics.Engagement.Controllers.V1.
// EngagementV1Controller, which is routed at "api/v{version}/analytics/engagement" (bare, no
// trailing segment) and serves a completely unrelated, pre-existing generic engagement-metric
// time-series query (PlantSafety.Application.Analytics.Queries.GetEngagementQuery) -- confirmed
// zero overlap by grepping both controllers' route templates before finalizing this route. A new
// controller class (rather than extending EngagementV1Controller) was chosen deliberately: adding
// action-level [Authorize(Policy=...)] on top of EngagementV1Controller's existing controller-level
// [Authorize(Policy = EngagementAuthorizationPolicies.Read)] would AND both policies together on
// every mutating action, which is unsafe to reason about without knowing whether the underlying
// Safety-tier roles are hierarchically inclusive. This mirrors exactly how PermitsV1Controller
// itself was added as a new controller alongside the 13 pre-existing read-only "permit type"
// category controllers rather than extending any of them.
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers().
// - Envelope fields (CommandId/CorrelationId/TenantId/ActorId etc.) are populated via the shared
//   RequestEnvelopeFactory rather than hand-rolled per action. ActorId always comes from the
//   authenticated caller (ICurrentUserService), never from client-supplied input.
// - All mutating actions use SafetyTierPolicies.Write; reads use SafetyTierPolicies.Read.
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Analytics.Engagement.Contracts.Requests;
using PlantSafety.Application.Analytics.Engagement.Commands;
using PlantSafety.Application.Analytics.Engagement.Queries;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.Analytics.Engagement.Controllers.V1;

/// <summary>V1 controller for the Analytics.Engagement module's lifecycle commands and queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/analytics/engagement/records")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Analytics.Engagement.Records")]
public sealed class EngagementRecordsV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="EngagementRecordsV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public EngagementRecordsV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Submits a new Engagement measurement.</summary>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CreateEngagementResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CreateEngagementResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateEngagementRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateEngagementCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            WorkerId: request.WorkerId,
            EngagementScore: request.EngagementScore,
            SurveyPeriod: request.SurveyPeriod,
            ParticipationChannel: request.ParticipationChannel,
            ResponseCount: request.ResponseCount,
            LastEngagedAtUtc: request.LastEngagedAtUtc,
            RequesterUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Reviews and activates an Engagement measurement (Created -&gt; Active).</summary>
    [HttpPost("{engagementId:guid}/review")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<EngagementStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EngagementStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ReviewAsync(
        Guid engagementId,
        [FromBody] ReviewEngagementRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ReviewEngagementCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            EngagementId: engagementId,
            ReviewedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records a worker touchpoint against an active Engagement measurement.</summary>
    [HttpPost("{engagementId:guid}/touchpoint")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<EngagementStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EngagementStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RecordTouchpointAsync(
        Guid engagementId,
        [FromBody] RecordEngagementTouchpointRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RecordEngagementTouchpointCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            EngagementId: engagementId,
            PerformedByUserId: env.ActorId,
            ResponseCount: request.ResponseCount)
        {
            ETag = request.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Completes an active Engagement measurement (Active -&gt; Completed).</summary>
    [HttpPost("{engagementId:guid}/complete")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<EngagementStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EngagementStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CompleteAsync(
        Guid engagementId,
        [FromBody] CompleteEngagementRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CompleteEngagementCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            EngagementId: engagementId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Formally closes a completed Engagement measurement (Completed -&gt; Closed).</summary>
    [HttpPost("{engagementId:guid}/close")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<EngagementStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EngagementStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CloseAsync(
        Guid engagementId,
        [FromBody] CloseEngagementRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CloseEngagementCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            EngagementId: engagementId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Archives a closed Engagement measurement for long-term retention (Closed -&gt; Archived).</summary>
    [HttpPost("{engagementId:guid}/archive")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<EngagementStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EngagementStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ArchiveAsync(
        Guid engagementId,
        [FromBody] ArchiveEngagementRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ArchiveEngagementCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            EngagementId: engagementId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single Engagement read-model record by identity, including its current ETag.</summary>
    [HttpGet("{engagementId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.Analytics.Engagement.Models.EngagementItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        Guid engagementId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetEngagementByIdQuery(env.TenantId, engagementId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns Engagement read-model data for a tenant/site within a date range.</summary>
    [HttpGet]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<EngagementRecordsQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EngagementRecordsQueryResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRecordsAsync(
        [FromQuery] EngagementRecordsListRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetEngagementRecordsQuery(
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            From: request.From ?? DateTime.UtcNow.AddDays(-90),
            To: request.To ?? DateTime.UtcNow.AddDays(90));

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
