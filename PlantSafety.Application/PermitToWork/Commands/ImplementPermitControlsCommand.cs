// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Command to record that required safety controls (LOTO, PPE, barricades,
// atmospheric testing) have been implemented for a permit (PTW Workflow Step 3).
// SchemaVersion: 1
// UPDATED: 2026-07-29 -- Implements IConcurrencyAwareCommand (Gap 2 remediation): callers should
//   supply the ETag captured from a prior read; a stale value results in ConcurrencyConflictException.

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Concurrency;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PermitToWork.Commands;

/// <summary>Records that required safety controls have been implemented for a PermitWorkflow (PTW Workflow Step 3).</summary>
public sealed record ImplementPermitControlsCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid PermitWorkflowId,
    string ControlsConfirmedBy,
    Guid PerformedByUserId) : ICommand<Result<PermitWorkflowActionResult>>, IConcurrencyAwareCommand
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<PermitWorkflowActionResult>>.CausationId => CausationId;

    /// <inheritdoc/>
    public string? ETag { get; init; }
}
