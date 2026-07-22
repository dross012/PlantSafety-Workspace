// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.PPEBehavioralMonitoring
//  Layer:          Domain
//  File:           PPEBehavioralMonitoringLifecycleRules.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the PPEBehavioralMonitoring aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.PPEBehavioralMonitoring.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="PPEBehavioralMonitoring"/> aggregate.</summary>
public static class PPEBehavioralMonitoringLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the aggregate may be activated from <paramref name="current"/>.</summary>
    public static bool CanActivate(PPEBehavioralMonitoringStatus current) => current == PPEBehavioralMonitoringStatus.Created;

    /// <summary>Returns <see langword="true"/> when the aggregate may be completed from <paramref name="current"/>.</summary>
    public static bool CanComplete(PPEBehavioralMonitoringStatus current) => current == PPEBehavioralMonitoringStatus.Active;

    /// <summary>Returns <see langword="true"/> when the aggregate may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(PPEBehavioralMonitoringStatus current) => current == PPEBehavioralMonitoringStatus.Completed;

    /// <summary>Returns <see langword="true"/> when the aggregate may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(PPEBehavioralMonitoringStatus current) => current == PPEBehavioralMonitoringStatus.Closed;
}