// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.EnvironmentalHealthMonitoring
//  Layer:          Domain
//  File:           EnvironmentalHealthMonitoringArchivalPolicy.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain policy governing archival eligibility for EnvironmentalHealthMonitoring aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.EnvironmentalHealthMonitoring.Policies;

/// <summary>Determines whether a <see cref="EnvironmentalHealthMonitoring"/> aggregate is eligible for archival.</summary>
public static class EnvironmentalHealthMonitoringArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the monitoring record may be archived (within limits or resolved).</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.EmployeeSafety.EnvironmentalHealthMonitoring.Aggregates.EnvironmentalHealthMonitoring aggregate)
        => aggregate.Status is EnvironmentalHealthMonitoringStatus.WithinLimits or EnvironmentalHealthMonitoringStatus.Resolved;
}
