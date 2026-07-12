// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard
//  Layer:          Domain
//  File:           DashboardArchivalPolicy.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for Dashboard aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Dashboard.Policies;

/// <summary>Determines whether a <see cref="Dashboard"/> aggregate is eligible for archival.</summary>
public static class DashboardArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the Dashboard may be archived.</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.Analytics.Dashboard.Aggregates.Dashboard aggregate)
        => aggregate.Status == DashboardStatus.Closed;
}
