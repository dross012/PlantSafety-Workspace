// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.AccumulationAreas
//  Layer:          Domain
//  File:           AccumulationAreaLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Defines permitted lifecycle transitions for the AccumulationArea aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.Enums;

namespace PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.Rules;

/// <summary>Defines permitted lifecycle transitions for the AccumulationArea aggregate.</summary>
public static class AccumulationAreaLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the area may be deactivated from <paramref name="current"/>.</summary>
    public static bool CanDeactivate(AccumulationAreaStatus current) =>
        current is AccumulationAreaStatus.Active or AccumulationAreaStatus.TimeLimitWarning;
}
