// ============================================================================
// File: AITrainingJobV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (Claude Code refactoring-agent)
// Created: 2026-08-02
// Description: V1 MVC controller exposing the Phase 4 full build-out's Create/lifecycle/by-id
//              command and query endpoints for the newly built AITrainingJob aggregate. Distinct
//              from the pre-existing PlantSafety.Api.Intelligence.AITraining.Controllers.V1
//              .AITrainingV1Controller (the untouched date-range read-model controller) --
//              different controller class name, different route segment
//              ("training-jobs" vs the read controller's own route), no path collision.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/intelligence/ai-training/training-jobs/...
//
// Rationale:
// - Routes all reads/writes through MediatR ISender to the Application layer. No domain logic or
//   data access here.
//
// ============================================================================

using PlantSafety.Api.Intelligence.AITraining.Contracts.Requests;
using PlantSafety.Api.Intelligence.AITraining.Authorization;
using PlantSafety.Application.Intelligence.AITraining.Commands;
using PlantSafety.Application.Intelligence.AITraining.Queries;

using PlantSafety.Api.Common;
using PlantSafety.Api.Context;

namespace PlantSafety.Api.Intelligence.AITraining.Controllers.V1;

/// <summary>
/// V1 controller for the Phase 4 full build-out's Create/lifecycle/by-id command and query
/// endpoints for the AITrainingJob aggregate.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/intelligence/ai-training/training-jobs")]
[Authorize(Policy = AITrainingAuthorizationPolicies.Read)]
[Tags("AITrainingJob")]
public sealed class AITrainingJobV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="AITrainingJobV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public AITrainingJobV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Creates a new Intelligence.AITraining training job record.</summary>
    [HttpPost]
    [Authorize(Policy = AITrainingAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<AITrainingJobStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AITrainingJobStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateAITrainingJobRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateAITrainingJobCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            ModelName: request.ModelName,
            ModelVersion: request.ModelVersion,
            DatasetReference: request.DatasetReference,
            InitiatedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Begins execution of a training job (Queued -&gt; Running).</summary>
    [HttpPost("{aiTrainingJobId:guid}/start")]
    [Authorize(Policy = AITrainingAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<AITrainingJobStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AITrainingJobStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> StartAsync(
        Guid aiTrainingJobId,
        [FromBody] StartAITrainingJobRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new StartAITrainingJobCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            AITrainingJobId: aiTrainingJobId,
            StartedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Marks a training job as completed successfully (Running -&gt; Completed).</summary>
    [HttpPost("{aiTrainingJobId:guid}/complete")]
    [Authorize(Policy = AITrainingAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<AITrainingJobStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AITrainingJobStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CompleteAsync(
        Guid aiTrainingJobId,
        [FromBody] CompleteAITrainingJobRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CompleteAITrainingJobCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            AITrainingJobId: aiTrainingJobId,
            PerformedByUserId: env.ActorId,
            ModelArtifactReference: request?.ModelArtifactReference)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Marks a training job as failed (Running -&gt; Failed).</summary>
    [HttpPost("{aiTrainingJobId:guid}/fail")]
    [Authorize(Policy = AITrainingAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<AITrainingJobStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AITrainingJobStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> FailAsync(
        Guid aiTrainingJobId,
        [FromBody] FailAITrainingJobRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new FailAITrainingJobCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            AITrainingJobId: aiTrainingJobId,
            PerformedByUserId: env.ActorId,
            FailureReason: request.FailureReason)
        {
            ETag = request.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Archives a completed or failed training job for compliance retention (Completed/Failed -&gt; Archived).</summary>
    [HttpPost("{aiTrainingJobId:guid}/archive")]
    [Authorize(Policy = AITrainingAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<AITrainingJobStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AITrainingJobStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ArchiveAsync(
        Guid aiTrainingJobId,
        [FromBody] ArchiveAITrainingJobRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ArchiveAITrainingJobCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            AITrainingJobId: aiTrainingJobId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single AITrainingJob read-model record by identity, including its current ETag.</summary>
    [HttpGet("{aiTrainingJobId:guid}")]
    [Authorize(Policy = AITrainingAuthorizationPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.Intelligence.AITraining.Models.AITrainingJobDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        Guid aiTrainingJobId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetAITrainingJobByIdQuery(env.TenantId, aiTrainingJobId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
