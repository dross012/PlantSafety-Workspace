// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEInventory
//  Layer:          Domain
//  File:           PPEInventoryLifecycleRules.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the PPEInventory aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEInventory.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="PPEInventory"/> aggregate.</summary>
public static class PPEInventoryLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the aggregate may be activated from <paramref name="current"/>.</summary>
    public static bool CanActivate(PPEInventoryStatus current) => current == PPEInventoryStatus.Created;

    /// <summary>Returns <see langword="true"/> when the aggregate may be completed from <paramref name="current"/>.</summary>
    public static bool CanComplete(PPEInventoryStatus current) => current == PPEInventoryStatus.Active;

    /// <summary>Returns <see langword="true"/> when the aggregate may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(PPEInventoryStatus current) => current == PPEInventoryStatus.Completed;

    /// <summary>Returns <see langword="true"/> when the aggregate may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(PPEInventoryStatus current) => current == PPEInventoryStatus.Closed;
}