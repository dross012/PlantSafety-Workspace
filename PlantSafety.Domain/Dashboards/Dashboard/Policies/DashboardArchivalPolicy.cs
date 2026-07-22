// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Dashboard
//  Layer:          Domain
//  File:           DashboardArchivalPolicy.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for Dashboard aggregates.
//                  Additive-only evolution.
// ============================================================================
using DashboardStatus = PlantSafety.Domain.Dashboards.Dashboard.Enums.DashboardStatus;

namespace PlantSafety.Domain.Dashboards.Dashboard.Policies;

/// <summary>Determines whether a <see cref="Aggregates.Dashboard"/> aggregate is eligible for archival.</summary>
public static class DashboardArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the Dashboard may be archived (not already archived).</summary>
    public static bool IsEligibleForArchival(Aggregates.Dashboard aggregate)
        => aggregate.Status != DashboardStatus.Archived;
}
