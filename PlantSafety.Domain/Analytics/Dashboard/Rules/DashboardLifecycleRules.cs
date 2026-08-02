// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard
//  Layer:          Domain
//  File:           DashboardLifecycleRules.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Dashboard.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="Dashboard"/> aggregate.</summary>
public static class DashboardLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the aggregate may be activated from <paramref name="current"/>.</summary>
    public static bool CanActivate(DashboardStatus current) => current == DashboardStatus.Created;

    /// <summary>Returns <see langword="true"/> when the aggregate may be completed from <paramref name="current"/>.</summary>
    public static bool CanComplete(DashboardStatus current) => current == DashboardStatus.Active;

    /// <summary>Returns <see langword="true"/> when the aggregate may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(DashboardStatus current) => current == DashboardStatus.Completed;

    /// <summary>Returns <see langword="true"/> when the aggregate may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(DashboardStatus current) => current == DashboardStatus.Closed;

    /// <summary>Returns <see langword="true"/> when the aggregate's data may be refreshed from <paramref name="current"/>.</summary>
    public static bool CanRefresh(DashboardStatus current) => current == DashboardStatus.Active;
}
