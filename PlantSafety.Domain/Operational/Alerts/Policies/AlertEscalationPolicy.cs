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

using PlantSafety.Domain.Operational.Alerts.Aggregates;
using PlantSafety.Domain.Operational.Alerts.Enums;

namespace PlantSafety.Domain.Operational.Alerts.Policies;

public sealed class AlertEscalationPolicy
{
    public bool CanEscalate(Alert alert)
    {
        // AlertStatus is a smart enum (sealed class with static readonly
        // instances), not a real C# enum, so its members are not compile-time
        // constants and cannot be used in constant/type patterns (CS9135).
        // Reference/record equality via `==` is used instead.
        if (alert.Status == AlertStatus.Resolved || alert.Status == AlertStatus.Closed || alert.Status == AlertStatus.Archived)
            return false;

        // Emergency & Critical alerts can always escalate
        if (alert.Severity.Code is "Emergency" or "Critical")
            return true;

        // High severity can escalate only if not resolved
        if (alert.Severity.Code == "High")
            return alert.Status == AlertStatus.Acknowledged || alert.Status == AlertStatus.InProgress;

        // Medium/Low severity cannot escalate
        return false;
    }
}
