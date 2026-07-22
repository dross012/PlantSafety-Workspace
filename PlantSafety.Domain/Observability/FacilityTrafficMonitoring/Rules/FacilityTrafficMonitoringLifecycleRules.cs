// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.FacilityTrafficMonitoring
//  Layer:          Domain
//  File:           FacilityTrafficMonitoringLifecycleRules.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the FacilityTrafficMonitoring aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.FacilityTrafficMonitoring.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="FacilityTrafficMonitoring"/> aggregate.</summary>
public static class FacilityTrafficMonitoringLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the aggregate may be activated from <paramref name="current"/>.</summary>
    public static bool CanActivate(FacilityTrafficMonitoringStatus current) => current == FacilityTrafficMonitoringStatus.Created;

    /// <summary>Returns <see langword="true"/> when the aggregate may be completed from <paramref name="current"/>.</summary>
    public static bool CanComplete(FacilityTrafficMonitoringStatus current) => current == FacilityTrafficMonitoringStatus.Active;

    /// <summary>Returns <see langword="true"/> when the aggregate may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(FacilityTrafficMonitoringStatus current) => current == FacilityTrafficMonitoringStatus.Completed;

    /// <summary>Returns <see langword="true"/> when the aggregate may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(FacilityTrafficMonitoringStatus current) => current == FacilityTrafficMonitoringStatus.Closed;
}