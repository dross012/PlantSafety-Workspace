// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.WasteStreams
//  Layer:          Domain
//  File:           WasteStreamLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Defines permitted lifecycle transitions for the WasteStream aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Enums;

namespace PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Rules;

/// <summary>Defines permitted lifecycle transitions for the WasteStream aggregate.</summary>
public static class WasteStreamLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the stream may be re-characterized from <paramref name="current"/>.</summary>
    public static bool CanRecharacterize(WasteStreamStatus current) => current == WasteStreamStatus.Characterized;

    /// <summary>Returns <see langword="true"/> when the stream may be deactivated from <paramref name="current"/>.</summary>
    public static bool CanDeactivate(WasteStreamStatus current) => current == WasteStreamStatus.Characterized;
}
