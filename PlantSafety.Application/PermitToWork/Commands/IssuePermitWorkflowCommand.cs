// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Command to issue an approved permit, making it active in the field
// (PTW Workflow Step 4/5).
// SchemaVersion: 1
// UPDATED: 2026-07-29 -- Implements IConcurrencyAwareCommand (Gap 2 remediation): callers should
//   supply the ETag captured from a prior read; a stale value results in ConcurrencyConflictException.

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Concurrency;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PermitToWork.Commands;

/// <summary>Issues an approved PermitWorkflow, making it active (PTW Workflow Step 4/5).</summary>
/// <param name="ValidityHours">Optional validity window, in hours, from issuance. Null means no computed expiration.</param>
/// <param name="CommandId">Unique identifier for this command instance.</param>
/// <param name="CorrelationId">Correlation identifier grouping every operation in the same business transaction.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event that caused this one.</param>
/// <param name="TenantId">Tenant the operation is scoped to.</param>
/// <param name="PermitWorkflowId">Identifier of the PermitWorkflow record.</param>
/// <param name="IssuerUserId">Identifier of the IssuerUser record.</param>
public sealed record IssuePermitWorkflowCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid PermitWorkflowId,
    Guid IssuerUserId,
    int? ValidityHours) : ICommand<Result<PermitWorkflowActionResult>>, IConcurrencyAwareCommand
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => IssuerUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<PermitWorkflowActionResult>>.CausationId => CausationId;

    /// <inheritdoc/>
    public string? ETag { get; init; }
}
