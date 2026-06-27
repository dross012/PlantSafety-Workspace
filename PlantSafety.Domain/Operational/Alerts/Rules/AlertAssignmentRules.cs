// ============================================================================
//  File:           AlertAssignmentRules.cs
//  Created:        2026-05-26
//  Description:
//      Domain rules for alert assignment. Ensures assignment is only allowed
//      in active states and that assignee identifiers are valid.
// ============================================================================


// ============================================================================
//  File:           AlertAssignmentRules.cs
//  Created:        2026-05-26
//  Description:
//      Domain rules for alert assignment. Ensures assignment is only allowed
//      in active states and that assignee identifiers are valid.
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Rules;

public static class AlertAssignmentRules
{
    public static bool CanAssign(AlertStatus status)
        => status != AlertStatus.Closed &&
           status != AlertStatus.Archived;

    public static bool IsValidAssignee(string assignee)
        => !string.IsNullOrWhiteSpace(assignee);
}
