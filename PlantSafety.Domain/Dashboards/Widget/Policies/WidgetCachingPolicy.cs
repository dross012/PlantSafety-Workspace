// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Widget
//  Layer:          Domain
//  File:           WidgetCachingPolicy.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain policy determining the canonical caching/TTL cadence for a Widget, per
//                  "6. Performance &amp; Real-Time Design: Heavy metrics (TRIR, DART, monthly
//                  trends) -> pre-aggregated. Real-time streams (alerts, observability, man-down,
//                  E-Stops) -> push updates. Caching at dashboard and widget level with
//                  configurable TTL."
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Dashboards.Widget.Enums;
using PlantSafety.Domain.Dashboards.Widget.ValueObjects;

namespace PlantSafety.Domain.Dashboards.Widget.Policies;

/// <summary>Determines the canonical caching cadence a <see cref="Aggregates.Widget"/> should use.</summary>
public static class WidgetCachingPolicy
{
    /// <summary>
    /// Returns the recommended refresh interval for a widget of the given type. Widget types that
    /// support real-time push updates default to <see cref="WidgetRefreshInterval.RealTime"/>;
    /// all others default to <see cref="WidgetRefreshInterval.HeavyMetric"/> (pre-aggregated).
    /// </summary>
    public static WidgetRefreshInterval RecommendedRefreshInterval(WidgetType type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return type.SupportsRealTimePush ? WidgetRefreshInterval.RealTime : WidgetRefreshInterval.HeavyMetric;
    }
}
