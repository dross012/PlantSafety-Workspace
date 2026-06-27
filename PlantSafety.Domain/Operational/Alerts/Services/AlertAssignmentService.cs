// ============================================================================
//  File:           AlertAssignmentService.cs
//  Created:        2026-05-26
//  Description:
//      Domain service for assignment logic. Enforces assignment rules and
//      emits domain events. No infrastructure logic.
// ============================================================================


namespace PlantSafety.Domain.Operational.Alerts.Services;

public sealed class AlertAssignmentService
{
    public static void Assign(
        Alert alert,
        string assignedTo,
        UtcTimestamp timestamp,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        if (!AlertAssignmentRules.CanAssign(alert.Status))
            throw new AlertAssignmentException(
                $"Cannot assign alert in state '{alert.Status}'",
                correlationId,
                causationId);

        if (!AlertAssignmentRules.IsValidAssignee(assignedTo))
            throw new AlertAssignmentException(
                "Invalid assignee identifier.",
                correlationId,
                causationId);

        alert.AssignTo(assignedTo, timestamp, actor, correlationId, causationId);
    }
}
