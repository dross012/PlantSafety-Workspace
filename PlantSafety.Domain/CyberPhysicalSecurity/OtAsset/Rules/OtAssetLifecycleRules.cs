// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.OtAsset
//  Layer:          Domain
//  File:           OtAssetLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Pure, stateless lifecycle-transition rules for the OtAsset aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.Enums;

namespace PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.Rules;

/// <summary>Pure lifecycle-transition rules for <see cref="Aggregates.OtAsset"/>.</summary>
public static class OtAssetLifecycleRules
{
    /// <summary>Canonical allowed status transition map.</summary>
    public static readonly IReadOnlyDictionary<OtAssetStatus, OtAssetStatus[]> AllowedTransitions =
        new Dictionary<OtAssetStatus, OtAssetStatus[]>
        {
            [OtAssetStatus.Active]         = [OtAssetStatus.UnderReview, OtAssetStatus.Quarantined, OtAssetStatus.Decommissioned],
            [OtAssetStatus.UnderReview]    = [OtAssetStatus.Active, OtAssetStatus.Quarantined, OtAssetStatus.Decommissioned],
            [OtAssetStatus.Quarantined]    = [OtAssetStatus.UnderReview, OtAssetStatus.Decommissioned],
            [OtAssetStatus.Decommissioned] = [],
        };

    /// <summary>Determines whether a transition from <paramref name="from"/> to <paramref name="to"/> is valid.</summary>
    public static bool IsValidTransition(OtAssetStatus from, OtAssetStatus to)
        => AllowedTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
}
