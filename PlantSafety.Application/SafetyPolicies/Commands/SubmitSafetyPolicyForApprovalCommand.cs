// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-21
// FILE DESCRIPTION: Command to submit a Draft SafetyPolicy into its approval chain. Carries the
//   full command envelope (CorrelationId, CausationId, CommandId, RequestedAtUtc, ApiVersionToken)
//   per Handbook Rule 2.1.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyPolicies.Commands;

/// <summary>Submits a Draft SafetyPolicy into its approval chain.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SafetyPolicyId">Identity of the policy being submitted.</param>
/// <param name="SubmittedBy">Identity of the user submitting the policy.</param>
public sealed record SubmitSafetyPolicyForApprovalCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    Guid TenantId,
    Guid SafetyPolicyId,
    Guid SubmittedBy) : ICommand<Result<SafetyPolicyStatusResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Maps to <see cref="SubmittedBy"/>, the user submitting the policy for approval.</summary>
    public Guid ActorId => SubmittedBy;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<SafetyPolicyStatusResult>>.CausationId => CausationId;
}

/// <summary>Approves a policy that is currently PendingApproval.</summary>
public sealed record ApproveSafetyPolicyCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    Guid TenantId,
    Guid SafetyPolicyId,
    Guid ApprovedBy) : ICommand<Result<SafetyPolicyStatusResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Maps to <see cref="ApprovedBy"/>, the user approving the policy.</summary>
    public Guid ActorId => ApprovedBy;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<SafetyPolicyStatusResult>>.CausationId => CausationId;
}

/// <summary>Rejects a policy that is currently PendingApproval, returning it to Draft.</summary>
public sealed record RejectSafetyPolicyCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    Guid TenantId,
    Guid SafetyPolicyId,
    Guid RejectedBy,
    string Reason) : ICommand<Result<SafetyPolicyStatusResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Maps to <see cref="RejectedBy"/>, the user rejecting the policy.</summary>
    public Guid ActorId => RejectedBy;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<SafetyPolicyStatusResult>>.CausationId => CausationId;
}

/// <summary>Publishes an Approved policy, making it effective and starting the review-cadence clock.</summary>
public sealed record PublishSafetyPolicyCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    Guid TenantId,
    Guid SafetyPolicyId,
    DateTime EffectiveAtUtc) : ICommand<Result<SafetyPolicyStatusResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.
    /// Placeholder: no dedicated actor field exists on this command; defaults to <see cref="Guid.Empty"/>.</summary>
    public Guid ActorId { get; init; } = Guid.Empty;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<SafetyPolicyStatusResult>>.CausationId => CausationId;
}

/// <summary>Records a stakeholder acknowledgment of the currently published policy revision.</summary>
public sealed record AcknowledgeSafetyPolicyCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    Guid TenantId,
    Guid SafetyPolicyId,
    Guid AcknowledgedBy) : ICommand<Result<SafetyPolicyStatusResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Maps to <see cref="AcknowledgedBy"/>, the user acknowledging the policy.</summary>
    public Guid ActorId => AcknowledgedBy;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<SafetyPolicyStatusResult>>.CausationId => CausationId;
}

/// <summary>Starts the mandated periodic review cycle for a Published policy.</summary>
public sealed record StartSafetyPolicyReviewCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    Guid TenantId,
    Guid SafetyPolicyId,
    Guid StartedBy) : ICommand<Result<SafetyPolicyStatusResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Maps to <see cref="StartedBy"/>, the user starting the review cycle.</summary>
    public Guid ActorId => StartedBy;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<SafetyPolicyStatusResult>>.CausationId => CausationId;
}

/// <summary>Completes a periodic review cycle, bumping the revision and republishing the policy.</summary>
public sealed record CompleteSafetyPolicyReviewCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    Guid TenantId,
    Guid SafetyPolicyId,
    Guid ReviewedBy,
    string? NewReviewCadence) : ICommand<Result<SafetyPolicyStatusResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Maps to <see cref="ReviewedBy"/>, the user completing the review cycle.</summary>
    public Guid ActorId => ReviewedBy;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<SafetyPolicyStatusResult>>.CausationId => CausationId;
}

/// <summary>Retires a Published or UnderReview policy, ending its governing force.</summary>
public sealed record RetireSafetyPolicyCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    Guid TenantId,
    Guid SafetyPolicyId,
    Guid RetiredBy,
    string Reason) : ICommand<Result<SafetyPolicyStatusResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Maps to <see cref="RetiredBy"/>, the user retiring the policy.</summary>
    public Guid ActorId => RetiredBy;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<SafetyPolicyStatusResult>>.CausationId => CausationId;
}

/// <summary>Result payload shared by all SafetyPolicy lifecycle-transition commands.</summary>
/// <param name="SafetyPolicyId">Identity of the affected policy.</param>
/// <param name="Status">Lifecycle status after the transition.</param>
/// <param name="SafetyPolicyVersion">Current policy revision number after the transition.</param>
public sealed record SafetyPolicyStatusResult(
    Guid SafetyPolicyId,
    string Status,
    int SafetyPolicyVersion);
