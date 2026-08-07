// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Classification
//  Layer:          Domain
//  File:           HazmatMaterialLifecycleRules.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Guards for the HazmatMaterial aggregate's classification lifecycle.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Classification.Enums;

namespace PlantSafety.Domain.Compliance.PHMSA.Classification.Rules;

/// <summary>Defines permitted lifecycle transitions for the <c>HazmatMaterial</c> aggregate.</summary>
public static class HazmatMaterialLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the aggregate may be superseded from <paramref name="current"/>.</summary>
    public static bool CanSupersede(HazmatMaterialStatus current) => current == HazmatMaterialStatus.Classified;

    /// <summary>Returns <see langword="true"/> when the aggregate may be retired from <paramref name="current"/>.</summary>
    public static bool CanRetire(HazmatMaterialStatus current) => current != HazmatMaterialStatus.Retired;
}
