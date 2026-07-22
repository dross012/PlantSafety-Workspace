// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.SiteOrientation
//  Layer:          Domain
//  File:           SiteOrientationLifecycleRules.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Defines permitted lifecycle transitions for the SiteOrientation aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.SiteOrientation.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="SiteOrientation"/> aggregate.</summary>
public static class SiteOrientationLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the orientation may be delivered from <paramref name="current"/>.</summary>
    public static bool CanDeliverOrientation(SiteOrientationStatus current) =>
        current is SiteOrientationStatus.Scheduled or SiteOrientationStatus.Expired;

    /// <summary>Returns <see langword="true"/> when comprehension may be verified from <paramref name="current"/>.</summary>
    public static bool CanVerifyComprehension(SiteOrientationStatus current) => current == SiteOrientationStatus.Delivered;

    /// <summary>Returns <see langword="true"/> when the orientation may expire from <paramref name="current"/>.</summary>
    public static bool CanExpire(SiteOrientationStatus current) => current == SiteOrientationStatus.ComprehensionVerified;
}
