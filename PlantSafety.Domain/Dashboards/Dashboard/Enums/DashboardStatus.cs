// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Dashboard
//  Layer:          Domain
//  File:           DashboardStatus.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Simple lifecycle enum for the Dashboard aggregate root. Pure classification
//                  with no metadata beyond ordering — satisfies the Simple Enum branch of the
//                  Enums Rule (2.7).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Dashboards.Dashboard.Enums;

/// <summary>Lifecycle states for the <see cref="Aggregates.Dashboard"/> aggregate root.</summary>
public enum DashboardStatus
{
    /// <summary>Dashboard definition has been drafted but is not yet visible to viewers.</summary>
    Draft = 0,

    /// <summary>Dashboard is published and visible to its permitted role(s).</summary>
    Published = 1,

    /// <summary>Dashboard has been archived and is no longer visible in navigation.</summary>
    Archived = 2,
}
