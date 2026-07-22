// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Alerts.Alert
//  Layer:          Domain
//  File:           AlertLifecycleRules.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the Alert aggregate.
//                  Additive-only evolution.
// ============================================================================
using AlertStatus = PlantSafety.Domain.Alerts.Alert.Enums.AlertStatus;

namespace PlantSafety.Domain.Alerts.Alert.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="Aggregates.Alert"/> aggregate.</summary>
public static class AlertLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the alert may be acknowledged from <paramref name="current"/>.</summary>
    public static bool CanAcknowledge(AlertStatus current)
        => current is AlertStatus.Triggered or AlertStatus.Escalated;

    /// <summary>Returns <see langword="true"/> when the alert may be dispatched from <paramref name="current"/>.</summary>
    public static bool CanDispatch(AlertStatus current)
        => current == AlertStatus.Acknowledged;

    /// <summary>Returns <see langword="true"/> when the alert may be escalated from <paramref name="current"/>.</summary>
    public static bool CanEscalate(AlertStatus current)
        => current is AlertStatus.Triggered or AlertStatus.Acknowledged or AlertStatus.Dispatched;

    /// <summary>Returns <see langword="true"/> when the alert may be resolved from <paramref name="current"/>.</summary>
    public static bool CanResolve(AlertStatus current)
        => current is AlertStatus.Acknowledged or AlertStatus.Dispatched or AlertStatus.Escalated;

    /// <summary>Returns <see langword="true"/> when the alert may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(AlertStatus current)
        => current == AlertStatus.Resolved;
}
