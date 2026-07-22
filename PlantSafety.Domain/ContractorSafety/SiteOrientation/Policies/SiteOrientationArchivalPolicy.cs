// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.SiteOrientation
//  Layer:          Domain
//  File:           SiteOrientationArchivalPolicy.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain policy governing archival eligibility for SiteOrientation aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.SiteOrientation.Policies;

/// <summary>Determines whether a <see cref="SiteOrientation"/> aggregate is eligible for archival.</summary>
public static class SiteOrientationArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the orientation record may be archived (expired only).</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.ContractorSafety.SiteOrientation.Aggregates.SiteOrientation aggregate)
        => aggregate.Status == SiteOrientationStatus.Expired;
}
