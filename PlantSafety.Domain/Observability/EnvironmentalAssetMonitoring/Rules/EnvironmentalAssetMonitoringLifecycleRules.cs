// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.EnvironmentalAssetMonitoring
//  Layer:          Domain
//  File:           EnvironmentalAssetMonitoringLifecycleRules.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the EnvironmentalAssetMonitoring aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.EnvironmentalAssetMonitoring.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="EnvironmentalAssetMonitoring"/> aggregate.</summary>
public static class EnvironmentalAssetMonitoringLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the aggregate may be activated from <paramref name="current"/>.</summary>
    public static bool CanActivate(EnvironmentalAssetMonitoringStatus current) => current == EnvironmentalAssetMonitoringStatus.Created;

    /// <summary>Returns <see langword="true"/> when the aggregate may be completed from <paramref name="current"/>.</summary>
    public static bool CanComplete(EnvironmentalAssetMonitoringStatus current) => current == EnvironmentalAssetMonitoringStatus.Active;

    /// <summary>Returns <see langword="true"/> when the aggregate may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(EnvironmentalAssetMonitoringStatus current) => current == EnvironmentalAssetMonitoringStatus.Completed;

    /// <summary>Returns <see langword="true"/> when the aggregate may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(EnvironmentalAssetMonitoringStatus current) => current == EnvironmentalAssetMonitoringStatus.Closed;
}