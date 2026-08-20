// ============================================================================
// File: ModelRegistryV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (Claude Code api-contract-validation-agent)
// Created: 2026-08-20
// Description: V1 MVC controller exposing the Register/SubmitForApproval/Approve/Deprecate/
//              Retire/by-id command and query endpoints for the Intelligence.ModelRegistry
//              ModelRegistryEntry aggregate. Prior to this file, the module's Application-layer
//              commands/queries/handlers (ported in the preceding repair stage) had no reachable
//              API surface -- no /Controllers folder existed under
//              PlantSafety.Api/Intelligence/ModelRegistry, unlike every sibling Intelligence
//              submodule (AIInvestigations, AITraining, DataLake, PredictiveSafety). Mirrors
//              PlantSafety.Api.Intelligence.DataLake.Controllers.V1.DataLakeActivityV1Controller.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/intelligence/model-registry/models/...
// - The Approve action uses a dedicated Approve authorization tier
//   (ModelRegistryAuthorizationPolicies.Approve, resolved to SafetyRoles.ApproverAndAbove) rather
//   than the general Write tier, because ApproveModelCommand is SAFETY-CRITICAL (Safety Rule
//   4.5) and its human-approver invariant must be enforced at the authorization boundary as well
//   as in the domain.
//
// Rationale:
// - Routes all reads/writes through MediatR ISender to the Application layer. No domain logic or
//   data access here.
//
// ============================================================================

using PlantSafety.Api.Intelligence.ModelRegistry.Contracts.Requests;
using PlantSafety.Api.Intelligence.ModelRegistry.Authorization;
using PlantSafety.Application.Intelligence.ModelRegistry.Commands;
using PlantSafety.Application.Intelligence.ModelRegistry.Queries;

using PlantSafety.Api.Common;
using PlantSafety.Api.Context;

namespace PlantSafety.Api.Intelligence.ModelRegistry.Controllers.V1;

/// <summary>
/// V1 controller for the Register/SubmitForApproval/Approve/Deprecate/Retire/by-id command and
/// query endpoints for the ModelRegistryEntry aggregate.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/intelligence/model-registry/models")]
[Authorize(Policy = ModelRegistryAuthorizationPolicies.Read)]
[Tags("ModelRegistry")]
public sealed class ModelRegistryV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="ModelRegistryV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public ModelRegistryV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Registers a new model version in the model registry.</summary>
    [HttpPost]
    [Authorize(Policy = ModelRegistryAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<ModelRegistryEntryStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ModelRegistryEntryStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RegisterAsync(
        [FromBody] RegisterModelRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RegisterModelCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            ModelName: request.ModelName,
            ModelVersion: request.ModelVersion,
            DatasetHash: request.DatasetHash,
            TrainingAccuracyScore: request.TrainingAccuracyScore,
            RegisteredByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Submits a Draft model registry entry for human approval (Draft -&gt; PendingApproval).</summary>
    [HttpPost("{modelRegistryEntryId:guid}/submit-for-approval")]
    [Authorize(Policy = ModelRegistryAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<ModelRegistryEntryStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ModelRegistryEntryStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SubmitForApprovalAsync(
        Guid modelRegistryEntryId,
        [FromBody] SubmitModelForApprovalRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new SubmitModelForApprovalCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            ModelRegistryEntryId: modelRegistryEntryId,
            SubmittedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>
    /// SAFETY-CRITICAL (Safety Rule 4.5): Approves a PendingApproval model registry entry for
    /// production deployment (PendingApproval -&gt; Approved). The approving user is always the
    /// authenticated human actor resolved from the request envelope -- never accepted from the
    /// request body -- so the domain's human-approver invariant cannot be spoofed by the caller.
    /// </summary>
    [HttpPost("{modelRegistryEntryId:guid}/approve")]
    [Authorize(Policy = ModelRegistryAuthorizationPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<ModelRegistryEntryStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ModelRegistryEntryStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ApproveAsync(
        Guid modelRegistryEntryId,
        [FromBody] ApproveModelRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ApproveModelCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            ModelRegistryEntryId: modelRegistryEntryId,
            ApprovedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Deprecates an Approved model registry entry (Approved -&gt; Deprecated).</summary>
    [HttpPost("{modelRegistryEntryId:guid}/deprecate")]
    [Authorize(Policy = ModelRegistryAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<ModelRegistryEntryStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ModelRegistryEntryStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeprecateAsync(
        Guid modelRegistryEntryId,
        [FromBody] DeprecateModelRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new DeprecateModelCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            ModelRegistryEntryId: modelRegistryEntryId,
            Reason: request.Reason,
            DeprecatedByUserId: env.ActorId)
        {
            ETag = request.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Permanently retires a model registry entry (terminal).</summary>
    [HttpPost("{modelRegistryEntryId:guid}/retire")]
    [Authorize(Policy = ModelRegistryAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<ModelRegistryEntryStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ModelRegistryEntryStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RetireAsync(
        Guid modelRegistryEntryId,
        [FromBody] RetireModelRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RetireModelCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            ModelRegistryEntryId: modelRegistryEntryId,
            RetiredByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single ModelRegistryEntry read-model record by identity, including its current ETag.</summary>
    [HttpGet("{modelRegistryEntryId:guid}")]
    [Authorize(Policy = ModelRegistryAuthorizationPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.Intelligence.ModelRegistry.Models.ModelRegistryEntryDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        Guid modelRegistryEntryId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetModelRegistryEntryByIdQuery(env.TenantId, modelRegistryEntryId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
