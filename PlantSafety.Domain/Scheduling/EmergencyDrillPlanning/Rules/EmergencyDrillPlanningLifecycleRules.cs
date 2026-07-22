// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.EmergencyDrillPlanning
//  Layer:          Domain
//  File:           EmergencyDrillPlanningLifecycleRules.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the EmergencyDrillPlanning aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="EmergencyDrillPlanning"/> aggregate.</summary>
public static class EmergencyDrillPlanningLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the aggregate may be activated from <paramref name="current"/>.</summary>
    public static bool CanActivate(EmergencyDrillPlanningStatus current) => current == EmergencyDrillPlanningStatus.Created;

    /// <summary>Returns <see langword="true"/> when the aggregate may be completed from <paramref name="current"/>.</summary>
    public static bool CanComplete(EmergencyDrillPlanningStatus current) => current == EmergencyDrillPlanningStatus.Active;

    /// <summary>Returns <see langword="true"/> when the aggregate may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(EmergencyDrillPlanningStatus current) => current == EmergencyDrillPlanningStatus.Completed;

    /// <summary>Returns <see langword="true"/> when the aggregate may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(EmergencyDrillPlanningStatus current) => current == EmergencyDrillPlanningStatus.Closed;
}