// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Alerts
//  Layer:          Domain
//  File:           AlertLifecycleRules.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Canonical lifecycle rules for Alerts. Defines all allowed transitions
//      and enforces the lifecycle defined in the Alerts Module Playbook:
//          Created → Acknowledged → InProgress → Escalated → Resolved → Closed → Archived
//      Complies with:
//          - Canonical Aggregate Rule (2.5)
//          - Business Model Fidelity Rule (4.4)
//          - Deterministic Behavior Rule
//      Additive-only evolution.
// ============================================================================


// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Alerts
//  Layer:          Domain
//  File:           AlertLifecycleRules.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Canonical lifecycle rules for Alerts. Defines all allowed transitions
//      and enforces the lifecycle defined in the Alerts Module Playbook:
//          Created → Acknowledged → InProgress → Escalated → Resolved → Closed → Archived
//      Complies with:
//          - Canonical Aggregate Rule (2.5)
//          - Business Model Fidelity Rule (4.4)
//          - Deterministic Behavior Rule
//      Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Rules;

public static class AlertLifecycleRules
{
    public static bool CanAcknowledge(AlertStatus current)
        => current == AlertStatus.Created;

    public static bool CanStartProgress(AlertStatus current)
        => current == AlertStatus.Created ||
           current == AlertStatus.Acknowledged;

    public static bool CanEscalate(AlertStatus current)
        => current == AlertStatus.Acknowledged ||
           current == AlertStatus.InProgress;

    public static bool CanResolve(AlertStatus current)
        => current == AlertStatus.InProgress ||
           current == AlertStatus.Escalated;

    public static bool CanClose(AlertStatus current)
        => current == AlertStatus.Resolved;

    public static bool CanArchive(AlertStatus current)
        => current == AlertStatus.Closed;
}
