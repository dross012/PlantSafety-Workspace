// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Widget
//  Layer:          Domain
//  File:           WidgetStatus.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Simple lifecycle enum for the Widget aggregate root. Pure classification with
//                  no metadata beyond ordering — satisfies the Simple Enum branch of the Enums
//                  Rule (2.7).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Dashboards.Widget.Enums;

/// <summary>Lifecycle states for the <see cref="Aggregates.Widget"/> aggregate root.</summary>
public enum WidgetStatus
{
    /// <summary>Widget definition has been drafted but is not yet available for use on dashboards.</summary>
    Draft = 0,

    /// <summary>Widget is active and may be placed on dashboards.</summary>
    Active = 1,

    /// <summary>Widget is deprecated; existing placements remain but new placements are discouraged.</summary>
    Deprecated = 2,

    /// <summary>Widget has been retired and may no longer be placed on dashboards.</summary>
    Retired = 3,
}
