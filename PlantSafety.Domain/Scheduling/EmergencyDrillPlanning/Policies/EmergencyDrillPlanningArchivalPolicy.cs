// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.EmergencyDrillPlanning
//  Layer:          Domain
//  File:           EmergencyDrillPlanningArchivalPolicy.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for EmergencyDrillPlanning aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Policies;

/// <summary>Determines whether a <see cref="EmergencyDrillPlanning"/> aggregate is eligible for archival.</summary>
public static class EmergencyDrillPlanningArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the EmergencyDrillPlanning may be archived.</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Aggregates.EmergencyDrillPlanning aggregate)
        => aggregate.Status == EmergencyDrillPlanningStatus.Closed;
}