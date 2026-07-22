// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.EnvironmentalHealthMonitoring
//  Layer:          Domain
//  File:           EnvironmentalHealthMonitoringLifecycleRules.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Defines permitted lifecycle transitions for the EnvironmentalHealthMonitoring aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.EnvironmentalHealthMonitoring.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="EnvironmentalHealthMonitoring"/> aggregate.</summary>
public static class EnvironmentalHealthMonitoringLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when a measurement may be recorded from <paramref name="current"/>.</summary>
    public static bool CanRecordMeasurement(EnvironmentalHealthMonitoringStatus current) =>
        current is EnvironmentalHealthMonitoringStatus.Scheduled or EnvironmentalHealthMonitoringStatus.WithinLimits or EnvironmentalHealthMonitoringStatus.Resolved;

    /// <summary>Returns <see langword="true"/> when corrective action may begin from <paramref name="current"/>.</summary>
    public static bool CanBeginCorrectiveAction(EnvironmentalHealthMonitoringStatus current) => current == EnvironmentalHealthMonitoringStatus.ExceedanceDetected;

    /// <summary>Returns <see langword="true"/> when an exceedance may be resolved from <paramref name="current"/>.</summary>
    public static bool CanResolveExceedance(EnvironmentalHealthMonitoringStatus current) => current == EnvironmentalHealthMonitoringStatus.CorrectiveActionInProgress;
}
