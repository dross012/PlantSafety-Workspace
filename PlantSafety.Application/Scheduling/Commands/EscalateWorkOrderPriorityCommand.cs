// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Command to escalate an overdue P1/P2 work order
// ("Automatic escalation for overdue P1/P2 items").
// SchemaVersion: 1
// UPDATED: 2026-07-30 -- Implements IConcurrencyAwareCommand (Phase 4
//   Scheduling.WorkOrderPriority concurrency remediation): callers should supply the ETag captured
//   from a prior read; a stale value results in ConcurrencyConflictException. Mirrors
//   RiskManagement's commands.

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Concurrency;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Scheduling.Commands;

/// <summary>Escalates an overdue work order.</summary>
public sealed record EscalateWorkOrderPriorityCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid WorkOrderPriorityId,
    string EscalationReason,
    Guid PerformedByUserId) : ICommand<Result<WorkOrderPriorityActionResult>>, IConcurrencyAwareCommand
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<WorkOrderPriorityActionResult>>.CausationId => CausationId;

    /// <inheritdoc/>
    public string? ETag { get; init; }
}
