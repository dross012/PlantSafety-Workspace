// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.PPEBehavioralMonitoring
//  Layer:          Domain
//  File:           PPEBehavioralMonitoringArchivalPolicy.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for PPEBehavioralMonitoring aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.PPEBehavioralMonitoring.Policies;

/// <summary>Determines whether a <see cref="PPEBehavioralMonitoring"/> aggregate is eligible for archival.</summary>
public static class PPEBehavioralMonitoringArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the PPEBehavioralMonitoring may be archived.</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.Observability.PPEBehavioralMonitoring.Aggregates.PPEBehavioralMonitoring aggregate)
        => aggregate.Status == PPEBehavioralMonitoringStatus.Closed;
}