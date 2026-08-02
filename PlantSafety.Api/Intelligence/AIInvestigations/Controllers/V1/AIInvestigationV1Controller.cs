// ============================================================================
// File: AIInvestigationV1Controller.cs
// Project: PlantSafety.Api
// Module: Intelligence.AIInvestigations
// Created: 2026-08-02
// Description: V1 controller for the Intelligence.AIInvestigations module's Phase 4 full
//              build-out Create/lifecycle/by-id command and query endpoints. This bounded context
//              previously had no controller at all -- only the pre-existing, unrelated read-only
//              AllInvestigationsV1Controller list-query vertical existed. Routes all reads/writes
//              through MediatR ISender to the Application layer. Mirrors
//              PlantSafety.Api.SafetyEvents.RootCauseAnalysis.Controllers.V1
//              .RootCauseAnalysisV1Controller.
// ============================================================================

using PlantSafety.Api.Intelligence.AIInvestigations.Authorization;
using PlantSafety.Api.Intelligence.AIInvestigations.Contracts.Requests;
using PlantSafety.Application.Intelligence.AIInvestigations.Commands;
using PlantSafety.Application.Intelligence.AIInvestigations.Queries;

using PlantSafety.Api.Common;
using PlantSafety.Api.Context;

namespace PlantSafety.Api.Intelligence.AIInvestigations.Controllers.V1;

/// <summary>V1 controller for the Intelligence.AIInvestigations module's write and by-id read endpoints.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/intelligence/ai-investigations")]
[Authorize(Policy = AIInvestigationAuthorizationPolicies.Read)]
[Tags("AIInvestigation")]
public sealed class AIInvestigationV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="AIInvestigationV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public AIInvestigationV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Opens a new AI-assisted safety investigation.</summary>
    [HttpPost]
    [Authorize(Policy = AIInvestigationAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<AIInvestigationStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AIInvestigationStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateAIInvestigationRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateAIInvestigationCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            IncidentReference: request.IncidentReference,
            AiConfidenceScore: request.AiConfidenceScore,
            Summary: request.Summary,
            AssignedToUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Starts review of an AIInvestigation (Open -&gt; InProgress).</summary>
    [HttpPost("{aiInvestigationId:guid}/start-review")]
    [Authorize(Policy = AIInvestigationAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<AIInvestigationStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AIInvestigationStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> StartReviewAsync(
        Guid aiInvestigationId,
        [FromBody] StartReviewAIInvestigationRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new StartReviewAIInvestigationCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            AIInvestigationId: aiInvestigationId,
            StartedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Closes an AIInvestigation (InProgress -&gt; Closed).</summary>
    [HttpPost("{aiInvestigationId:guid}/close")]
    [Authorize(Policy = AIInvestigationAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<AIInvestigationStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AIInvestigationStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CloseAsync(
        Guid aiInvestigationId,
        [FromBody] CloseAIInvestigationRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CloseAIInvestigationCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            AIInvestigationId: aiInvestigationId,
            ClosedByUserId: env.ActorId,
            ResolutionSummary: request?.ResolutionSummary)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Archives a closed AIInvestigation (Closed -&gt; Archived).</summary>
    [HttpPost("{aiInvestigationId:guid}/archive")]
    [Authorize(Policy = AIInvestigationAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<AIInvestigationStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AIInvestigationStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ArchiveAsync(
        Guid aiInvestigationId,
        [FromBody] ArchiveAIInvestigationRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ArchiveAIInvestigationCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            AIInvestigationId: aiInvestigationId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single AIInvestigation read-model record by identity, including its current ETag.</summary>
    [HttpGet("{aiInvestigationId:guid}")]
    [Authorize(Policy = AIInvestigationAuthorizationPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.Intelligence.AIInvestigations.Models.AIInvestigationDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        Guid aiInvestigationId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetAIInvestigationByIdQuery(env.TenantId, aiInvestigationId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
