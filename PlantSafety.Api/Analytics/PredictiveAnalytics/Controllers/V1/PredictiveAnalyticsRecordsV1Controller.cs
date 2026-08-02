// ============================================================================
// File: PredictiveAnalyticsRecordsV1Controller.cs
// Project: PlantSafety.Api
// Module: Analytics.PredictiveAnalytics
// Created: 2026-08-02
// Description: V1 controller exposing the Application-layer Analytics.PredictiveAnalytics
//              module's lifecycle Commands and Queries over HTTP. Covers the full lifecycle:
//              Create -> Review -> Action -> Close -> Archive. Mirrors
//              PlantSafety.Api.Compliance.Permits.Controllers.V1.PermitsV1Controller.
//
// Route note: this controller's route ("api/v{version}/analytics/predictiveanalytics/records")
// does not collide with the pre-existing PlantSafety.Api.Analytics.PredictiveAnalytics.
// Controllers.V1.PredictiveAnalyticsV1Controller, which is routed at
// "api/v{version}/analytics/predictiveanalytics" (bare, no trailing segment) and serves the
// pre-existing date-range list query (GetPredictiveAnalyticsQuery). A new controller class
// (rather than extending PredictiveAnalyticsV1Controller) was chosen deliberately, for the same
// reason documented on EngagementRecordsV1Controller: adding action-level
// [Authorize(Policy=...)] on top of that controller's existing controller-level
// [Authorize(Policy = PredictiveAnalyticsAuthorizationPolicies.Read)] would AND both policies
// together on every mutating action.
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers().
// - Envelope fields (CommandId/CorrelationId/TenantId/ActorId etc.) are populated via the shared
//   RequestEnvelopeFactory rather than hand-rolled per action. ActorId always comes from the
//   authenticated caller (ICurrentUserService), never from client-supplied input.
// - All mutating actions use SafetyTierPolicies.Write; reads use SafetyTierPolicies.Read.
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Analytics.PredictiveAnalytics.Contracts.Requests;
using PlantSafety.Application.Analytics.PredictiveAnalytics.Commands;
using PlantSafety.Application.Analytics.PredictiveAnalytics.Queries;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.Analytics.PredictiveAnalytics.Controllers.V1;

/// <summary>V1 controller for the Analytics.PredictiveAnalytics module's lifecycle commands and queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/analytics/predictiveanalytics/records")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Analytics.PredictiveAnalytics.Records")]
public sealed class PredictiveAnalyticsRecordsV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="PredictiveAnalyticsRecordsV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public PredictiveAnalyticsRecordsV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Submits a new PredictiveAnalytics record.</summary>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CreatePredictiveAnalyticsResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CreatePredictiveAnalyticsResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreatePredictiveAnalyticsRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreatePredictiveAnalyticsCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            ModelName: request.ModelName,
            PredictionType: request.PredictionType,
            TargetEntityId: request.TargetEntityId,
            PredictedRiskScore: request.PredictedRiskScore,
            ConfidenceLevel: request.ConfidenceLevel,
            PredictionHorizonDays: request.PredictionHorizonDays,
            RequesterUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Reviews a generated prediction (Created -&gt; Active, i.e. "Reviewed").</summary>
    [HttpPost("{predictiveAnalyticsId:guid}/review")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PredictiveAnalyticsStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PredictiveAnalyticsStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ReviewAsync(
        Guid predictiveAnalyticsId,
        [FromBody] ReviewPredictiveAnalyticsRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ReviewPredictiveAnalyticsCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PredictiveAnalyticsId: predictiveAnalyticsId,
            ReviewedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records that a reviewed prediction has been acted upon (Active -&gt; Completed, i.e. "Actioned").</summary>
    [HttpPost("{predictiveAnalyticsId:guid}/action")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PredictiveAnalyticsStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PredictiveAnalyticsStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ActionAsync(
        Guid predictiveAnalyticsId,
        [FromBody] ActionPredictiveAnalyticsRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ActionPredictiveAnalyticsCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PredictiveAnalyticsId: predictiveAnalyticsId,
            ActionedByUserId: env.ActorId,
            ActionNotes: request?.ActionNotes)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Formally closes an actioned prediction (Completed -&gt; Closed).</summary>
    [HttpPost("{predictiveAnalyticsId:guid}/close")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PredictiveAnalyticsStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PredictiveAnalyticsStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CloseAsync(
        Guid predictiveAnalyticsId,
        [FromBody] ClosePredictiveAnalyticsRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ClosePredictiveAnalyticsCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PredictiveAnalyticsId: predictiveAnalyticsId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Archives a closed prediction for long-term retention (Closed -&gt; Archived).</summary>
    [HttpPost("{predictiveAnalyticsId:guid}/archive")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PredictiveAnalyticsStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PredictiveAnalyticsStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ArchiveAsync(
        Guid predictiveAnalyticsId,
        [FromBody] ArchivePredictiveAnalyticsRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ArchivePredictiveAnalyticsCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PredictiveAnalyticsId: predictiveAnalyticsId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single PredictiveAnalytics read-model record by identity, including its current ETag.</summary>
    [HttpGet("{predictiveAnalyticsId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.Analytics.PredictiveAnalytics.Models.PredictiveAnalyticsItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        Guid predictiveAnalyticsId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetPredictiveAnalyticsByIdQuery(env.TenantId, predictiveAnalyticsId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
