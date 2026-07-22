// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEInventory
//  Layer:          Domain
//  File:           PPEInventoryArchivalPolicy.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for PPEInventory aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEInventory.Policies;

/// <summary>Determines whether a <see cref="PPEInventory"/> aggregate is eligible for archival.</summary>
public static class PPEInventoryArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the PPEInventory may be archived.</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.PPE.PPEInventory.Aggregates.PPEInventory aggregate)
        => aggregate.Status == PPEInventoryStatus.Closed;
}