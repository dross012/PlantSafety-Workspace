// ============================================================================
// File: SafetyPoliciesV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (Claude Code modular-creation-agent)
// Created: 2026-07-21
// Description: V1 MVC controller exposing every SafetyPolicies module command (create +
//              8 lifecycle-transition commands) over HTTP.
// ============================================================================
//
// Onboarding Notes:
// - Envelope fields are populated via RequestEnvelopeFactory, mirroring the canonical
//   Features/Compliance/Permits pattern. Commands are dispatched through MediatR ISender.
// - Responses use the shared ResultResponseExtensions.ToActionResult helper to translate the
//   Application layer's Result<T>/ApplicationError outcome into ApiResponse<T> + HTTP status.
//
// ============================================================================

using PlantSafety.Api.Common;
using PlantSafety.Api.Context;
using PlantSafety.Api.SafetyPolicies.Contracts.Requests;
using PlantSafety.Application.SafetyPolicies.Commands;

namespace PlantSafety.Api.SafetyPolicies.Controllers.V1;

/// <summary>V1 controller for SafetyPolicies lifecycle commands.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/safetypolicies")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("SafetyPolicies")]
public sealed class SafetyPoliciesV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="SafetyPoliciesV1Controller"/>.</summary>
    public SafetyPoliciesV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Creates a new SafetyPolicy record in Draft status.</summary>
    /// <response code="200">Policy created successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="422">Request validation failed.</response>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CreateSafetyPolicyResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<CreateSafetyPolicyResult>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateSafetyPolicyRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = CreateSafetyPolicyCommand.FromDto(
            tenantId: env.TenantId,
            siteId: request.SiteId,
            safetyPolicyTitle: request.SafetyPolicyTitle,
            category: request.Category,
            ownerId: request.OwnerId,
            effectiveAtUtc: request.EffectiveAtUtc,
            reviewRequired: request.ReviewRequired,
            correlationId: env.CorrelationId,
            causationId: env.CausationId,
            apiVersionToken: env.ApiVersionToken) with { ActorId = env.ActorId };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Submits a Draft SafetyPolicy into its approval chain.</summary>
    /// <response code="200">Policy submitted successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="422">Request or state transition invalid.</response>
    [HttpPost("{safetyPolicyId:guid}/submit-for-approval")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<SafetyPolicyStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<SafetyPolicyStatusResult>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> SubmitForApprovalAsync(Guid safetyPolicyId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new SubmitSafetyPolicyForApprovalCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.RequestedAtUtc,
            env.TenantId, safetyPolicyId, env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Approves a policy that is currently PendingApproval.</summary>
    /// <response code="200">Policy approved successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="422">Request or state transition invalid.</response>
    [HttpPost("{safetyPolicyId:guid}/approve")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<SafetyPolicyStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<SafetyPolicyStatusResult>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ApproveAsync(Guid safetyPolicyId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ApproveSafetyPolicyCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.RequestedAtUtc,
            env.TenantId, safetyPolicyId, env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Rejects a policy that is currently PendingApproval, returning it to Draft.</summary>
    /// <response code="200">Policy rejected successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="422">Request or state transition invalid.</response>
    [HttpPost("{safetyPolicyId:guid}/reject")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<SafetyPolicyStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<SafetyPolicyStatusResult>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RejectAsync(Guid safetyPolicyId, [FromBody] RejectSafetyPolicyRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RejectSafetyPolicyCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.RequestedAtUtc,
            env.TenantId, safetyPolicyId, env.ActorId, request.Reason);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Publishes an Approved policy, making it effective and starting the review-cadence clock.</summary>
    /// <response code="200">Policy published successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="422">Request or state transition invalid.</response>
    [HttpPost("{safetyPolicyId:guid}/publish")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<SafetyPolicyStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<SafetyPolicyStatusResult>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> PublishAsync(Guid safetyPolicyId, [FromBody] PublishSafetyPolicyRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new PublishSafetyPolicyCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.RequestedAtUtc,
            env.TenantId, safetyPolicyId, request.EffectiveAtUtc) { ActorId = env.ActorId };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records a stakeholder acknowledgment of the currently published policy revision.</summary>
    /// <response code="200">Acknowledgment recorded successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="422">Request or state transition invalid.</response>
    [HttpPost("{safetyPolicyId:guid}/acknowledge")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<SafetyPolicyStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<SafetyPolicyStatusResult>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AcknowledgeAsync(Guid safetyPolicyId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new AcknowledgeSafetyPolicyCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.RequestedAtUtc,
            env.TenantId, safetyPolicyId, env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Starts the mandated periodic review cycle for a Published policy.</summary>
    /// <response code="200">Review cycle started successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="422">Request or state transition invalid.</response>
    [HttpPost("{safetyPolicyId:guid}/start-review")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<SafetyPolicyStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<SafetyPolicyStatusResult>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> StartReviewAsync(Guid safetyPolicyId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new StartSafetyPolicyReviewCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.RequestedAtUtc,
            env.TenantId, safetyPolicyId, env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Completes a periodic review cycle, bumping the revision and republishing the policy.</summary>
    /// <response code="200">Review completed successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="422">Request or state transition invalid.</response>
    [HttpPost("{safetyPolicyId:guid}/complete-review")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<SafetyPolicyStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<SafetyPolicyStatusResult>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CompleteReviewAsync(Guid safetyPolicyId, [FromBody] CompleteSafetyPolicyReviewRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CompleteSafetyPolicyReviewCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.RequestedAtUtc,
            env.TenantId, safetyPolicyId, env.ActorId, request.NewReviewCadence);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retires a Published or UnderReview policy, ending its governing force.</summary>
    /// <response code="200">Policy retired successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="422">Request or state transition invalid.</response>
    [HttpPost("{safetyPolicyId:guid}/retire")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<SafetyPolicyStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<SafetyPolicyStatusResult>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RetireAsync(Guid safetyPolicyId, [FromBody] RetireSafetyPolicyRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RetireSafetyPolicyCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.RequestedAtUtc,
            env.TenantId, safetyPolicyId, env.ActorId, request.Reason);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
