// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.WasteContainers
//  Layer:          Domain
//  File:           WasteContainerLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Defines permitted lifecycle transitions for the WasteContainer aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.WasteContainers.Enums;

namespace PlantSafety.Domain.HazardousWasteManagement.WasteContainers.Rules;

/// <summary>Defines permitted lifecycle transitions for the WasteContainer aggregate.</summary>
public static class WasteContainerLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the container may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(WasteContainerStatus current) => current == WasteContainerStatus.Open;

    /// <summary>Returns <see langword="true"/> when the container may be marked shipped from <paramref name="current"/>.</summary>
    public static bool CanMarkShipped(WasteContainerStatus current) => current == WasteContainerStatus.Closed;
}
