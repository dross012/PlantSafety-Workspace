// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Widget
//  Layer:          Domain
//  File:           WidgetLifecycleRules.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the Widget aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Dashboards.Widget.Enums;

namespace PlantSafety.Domain.Dashboards.Widget.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="Aggregates.Widget"/> aggregate.</summary>
public static class WidgetLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the widget may be activated from <paramref name="current"/>.</summary>
    public static bool CanActivate(WidgetStatus current) => current == WidgetStatus.Draft;

    /// <summary>Returns <see langword="true"/> when the widget may be deprecated from <paramref name="current"/>.</summary>
    public static bool CanDeprecate(WidgetStatus current) => current == WidgetStatus.Active;

    /// <summary>Returns <see langword="true"/> when the widget may be retired from <paramref name="current"/>.</summary>
    public static bool CanRetire(WidgetStatus current) => current is WidgetStatus.Active or WidgetStatus.Deprecated;
}
