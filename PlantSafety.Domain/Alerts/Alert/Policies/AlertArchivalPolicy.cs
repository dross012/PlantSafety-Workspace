// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Alerts.Alert
//  Layer:          Domain
//  File:           AlertArchivalPolicy.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for Alert aggregates.
//                  Additive-only evolution.
// ============================================================================
using AlertStatus = PlantSafety.Domain.Alerts.Alert.Enums.AlertStatus;

namespace PlantSafety.Domain.Alerts.Alert.Policies;

/// <summary>Determines whether an <see cref="Aggregates.Alert"/> aggregate is eligible for archival.</summary>
public static class AlertArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the Alert may be archived (closed with close-out verification recorded).</summary>
    public static bool IsEligibleForArchival(Aggregates.Alert aggregate)
        => aggregate.Status == AlertStatus.Closed && !string.IsNullOrWhiteSpace(aggregate.CloseOutVerification);
}
