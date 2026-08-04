// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.OtAsset
//  Layer:          Domain
//  File:           OtAssetSpecification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Base specification abstraction for querying OtAsset aggregates in-memory.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.Enums;

namespace PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.Specifications;

/// <summary>Base specification for querying <see cref="Aggregates.OtAsset"/> aggregates.</summary>
public abstract class OtAssetSpecification
{
    /// <summary>Converts this specification into an in-memory predicate.</summary>
    public abstract Func<Aggregates.OtAsset, bool> ToPredicate();
}

/// <summary>Matches OtAssets with the given lifecycle status.</summary>
public sealed class OtAssetsByStatusSpecification(OtAssetStatus status) : OtAssetSpecification
{
    /// <inheritdoc/>
    public override Func<Aggregates.OtAsset, bool> ToPredicate() => a => a.Status == status;
}

/// <summary>Matches OtAssets at or above the given criticality rating.</summary>
public sealed class OtAssetsByMinimumCriticalitySpecification(OtAssetCriticality minimumCriticality) : OtAssetSpecification
{
    /// <inheritdoc/>
    public override Func<Aggregates.OtAsset, bool> ToPredicate() => a => a.Criticality >= minimumCriticality;
}
