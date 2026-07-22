// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Shared result payload for WorkOrderPriority lifecycle commands (start work,
// escalate, resolve, close).
// SchemaVersion: 1

namespace PlantSafety.Application.Scheduling.Commands;

/// <summary>Result of a successful WorkOrderPriority lifecycle command execution.</summary>
/// <param name="WorkOrderPriorityId">Identity of the affected WorkOrderPriority aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record WorkOrderPriorityActionResult(
    Guid WorkOrderPriorityId,
    string Status);
