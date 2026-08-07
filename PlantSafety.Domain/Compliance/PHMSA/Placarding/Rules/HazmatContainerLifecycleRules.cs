// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Placarding
//  Layer:          Domain
//  File:           HazmatContainerLifecycleRules.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Guards for the HazmatContainer aggregate's Draft -> MarkedAndLabeled ->
//                  PlacardingCalculated -> Retired lifecycle.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Placarding.Enums;

namespace PlantSafety.Domain.Compliance.PHMSA.Placarding.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="Aggregates.HazmatContainer"/> aggregate.</summary>
public static class HazmatContainerLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when markings/labels may be applied from <paramref name="current"/>.</summary>
    public static bool CanApplyMarkingsAndLabels(HazmatContainerStatus current) => current == HazmatContainerStatus.Draft;

    /// <summary>Returns <see langword="true"/> when placards may be applied from <paramref name="current"/>.</summary>
    public static bool CanApplyPlacards(HazmatContainerStatus current) => current == HazmatContainerStatus.MarkedAndLabeled;

    /// <summary>Returns <see langword="true"/> when the container may be retired from <paramref name="current"/>.</summary>
    public static bool CanRetire(HazmatContainerStatus current) => current != HazmatContainerStatus.Retired;
}
