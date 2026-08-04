// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessAnomalies
//  Layer:          Domain
//  File:           AccessAnomalyLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Defines permitted triage transitions for the AccessAnomaly aggregate,
//                  including the restricted-zone-violation auto-escalation policy.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Rules;

/// <summary>Defines permitted triage transitions for the <see cref="AccessAnomaly"/> aggregate.</summary>
public static class AccessAnomalyLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the anomaly may be acknowledged from <paramref name="current"/>.</summary>
    public static bool CanAcknowledge(AccessAnomalyStatus current) => current == AccessAnomalyStatus.Detected;

    /// <summary>Returns <see langword="true"/> when the anomaly may be escalated from <paramref name="current"/>.</summary>
    public static bool CanEscalate(AccessAnomalyStatus current) =>
        current is AccessAnomalyStatus.Detected or AccessAnomalyStatus.Acknowledged;

    /// <summary>Returns <see langword="true"/> when the anomaly may be resolved from <paramref name="current"/>.</summary>
    public static bool CanResolve(AccessAnomalyStatus current) =>
        current is AccessAnomalyStatus.Acknowledged or AccessAnomalyStatus.Escalated;

    /// <summary>
    /// Restricted-zone violations auto-escalate via AlertEscalationPolicy regardless of
    /// severity score; all other anomaly types escalate only above the severity threshold.
    /// </summary>
    public static bool ShouldAutoEscalate(AccessAnomalyType type, decimal severityScore) =>
        type == AccessAnomalyType.RestrictedZoneViolation || severityScore >= 75m;
}
