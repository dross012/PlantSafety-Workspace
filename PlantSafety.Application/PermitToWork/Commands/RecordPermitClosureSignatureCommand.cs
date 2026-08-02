// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Command to record the post-work review and closure signature for a permit,
// closing it (PTW Workflow Step 6 - Close-Out).
// SchemaVersion: 1
// UPDATED: 2026-07-29 -- Implements IConcurrencyAwareCommand (Gap 2 remediation): callers should
//   supply the ETag captured from a prior read; a stale value results in ConcurrencyConflictException.

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Concurrency;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PermitToWork.Commands;

/// <summary>Records the closure signature for an active PermitWorkflow, closing it (PTW Workflow Step 6).</summary>
public sealed record RecordPermitClosureSignatureCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid PermitWorkflowId,
    Guid ClosureSignedByUserId,
    string? ClosureNotes) : ICommand<Result<PermitWorkflowActionResult>>, IConcurrencyAwareCommand
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => ClosureSignedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<PermitWorkflowActionResult>>.CausationId => CausationId;

    /// <inheritdoc/>
    public string? ETag { get; init; }
}
