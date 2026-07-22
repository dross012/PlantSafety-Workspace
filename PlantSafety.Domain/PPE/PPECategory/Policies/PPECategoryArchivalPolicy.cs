// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPECategory
//  Layer:          Domain
//  File:           PPECategoryArchivalPolicy.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for PPECategory aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPECategory.Policies;

/// <summary>Determines whether a <see cref="PPECategory"/> aggregate is eligible for archival.</summary>
public static class PPECategoryArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the PPECategory may be archived.</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.PPE.PPECategory.Aggregates.PPECategory aggregate)
        => aggregate.Status == PPECategoryStatus.Closed;
}