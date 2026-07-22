// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Alerts.Alert
//  Layer:          Domain
//  File:           AlertEscalationPolicy.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain policy determining whether an unacknowledged Alert must be
//                  auto-escalated, based on its severity's AutoEscalationThresholdMinutes.
//                  Additive-only evolution.
// ============================================================================
using AlertStatus = PlantSafety.Domain.Alerts.Alert.Enums.AlertStatus;

namespace PlantSafety.Domain.Alerts.Alert.Policies;

/// <summary>Determines whether an <see cref="Aggregates.Alert"/> is eligible for auto-escalation.</summary>
public static class AlertEscalationPolicy
{
    /// <summary>
    /// Returns <see langword="true"/> when the alert remains unacknowledged (Triggered) or
    /// undispatched (Acknowledged) beyond its severity's auto-escalation threshold.
    /// </summary>
    public static bool RequiresAutoEscalation(Aggregates.Alert aggregate, UtcTimestamp nowUtc)
    {
        if (aggregate.Status is not (AlertStatus.Triggered or AlertStatus.Acknowledged))
            return false;

        var elapsed = nowUtc.Value - aggregate.TriggeredAtUtc.Value;
        return elapsed.TotalMinutes >= aggregate.Severity.AutoEscalationThresholdMinutes;
    }
}
