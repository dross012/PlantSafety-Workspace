// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Widget
//  Layer:          Domain
//  File:           WidgetDeprecationPolicy.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing retirement eligibility for deprecated Widget
//                  aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Dashboards.Widget.Enums;

namespace PlantSafety.Domain.Dashboards.Widget.Policies;

/// <summary>Determines whether a deprecated <see cref="Aggregates.Widget"/> is eligible for retirement.</summary>
public static class WidgetDeprecationPolicy
{
    /// <summary>Returns <see langword="true"/> when the widget may be retired (already deprecated).</summary>
    public static bool IsEligibleForRetirement(Aggregates.Widget aggregate)
        => aggregate.Status == WidgetStatus.Deprecated;
}
