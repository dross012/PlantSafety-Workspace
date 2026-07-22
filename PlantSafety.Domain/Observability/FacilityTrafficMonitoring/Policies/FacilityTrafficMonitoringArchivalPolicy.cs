// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.FacilityTrafficMonitoring
//  Layer:          Domain
//  File:           FacilityTrafficMonitoringArchivalPolicy.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for FacilityTrafficMonitoring aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.FacilityTrafficMonitoring.Policies;

/// <summary>Determines whether a <see cref="FacilityTrafficMonitoring"/> aggregate is eligible for archival.</summary>
public static class FacilityTrafficMonitoringArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the FacilityTrafficMonitoring may be archived.</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.Observability.FacilityTrafficMonitoring.Aggregates.FacilityTrafficMonitoring aggregate)
        => aggregate.Status == FacilityTrafficMonitoringStatus.Closed;
}