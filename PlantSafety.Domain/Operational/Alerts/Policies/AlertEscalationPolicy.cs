// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Alerts
//  Layer:          Domain
//  File:           AlertEscalationPolicy.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Domain policy defining escalation rules for Alerts. Ensures escalation
//      is allowed only in valid lifecycle states and when severity warrants it.
//      Complies with Domain Policy Rule (2.12).
// ============================================================================

using PlantSafety.Operational.Alerts.Domain.Alerts.Aggregates;
using PlantSafety.Operational.Alerts.Domain.Alerts.Enums;

namespace PlantSafety.Domain.Operational.Alerts.Policies;

public sealed class AlertEscalationPolicy
{
    public bool CanEscalate(Alert alert)
    {
        if (alert.Status is AlertStatus.Resolved or AlertStatus.Closed or AlertStatus.Archived)
            return false;

        // Emergency & Critical alerts can always escalate
        if (alert.Severity.Code is "Emergency" or "Critical")
            return true;

        // High severity can escalate only if not resolved
        if (alert.Severity.Code == "High")
            return alert.Status is AlertStatus.Acknowledged or AlertStatus.InProgress;

        // Medium/Low severity cannot escalate
        return false;
    }
}
