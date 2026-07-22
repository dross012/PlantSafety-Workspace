// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.EnvironmentalAssetMonitoring
//  Layer:          Domain
//  File:           EnvironmentalAssetMonitoringArchivalPolicy.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for EnvironmentalAssetMonitoring aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.EnvironmentalAssetMonitoring.Policies;

/// <summary>Determines whether a <see cref="EnvironmentalAssetMonitoring"/> aggregate is eligible for archival.</summary>
public static class EnvironmentalAssetMonitoringArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the EnvironmentalAssetMonitoring may be archived.</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.Observability.EnvironmentalAssetMonitoring.Aggregates.EnvironmentalAssetMonitoring aggregate)
        => aggregate.Status == EnvironmentalAssetMonitoringStatus.Closed;
}