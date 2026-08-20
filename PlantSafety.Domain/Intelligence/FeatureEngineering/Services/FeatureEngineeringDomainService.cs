// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.FeatureEngineering
//  Layer:          Domain
//  File:           FeatureEngineeringDomainService.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain service for feature engineering business rules: feature-set versioning
//                  (Supersedes) and publish-readiness (IsReadyToPublish). Ported (rebuilt,
//                  preserving the exact business logic) from the orphaned, dead
//                  Intelligence.Domain/FeatureEngineering/Services/FeatureEngineeringDomainService.cs
//                  onto this module's own FeatureSet entity/FeatureVersion value object and
//                  PlantSafety.Domain.Common.Services.IDomainService marker interface (stateless --
//                  no I/O permitted per that interface's contract) instead of the orphan's local
//                  Intelligence.Domain.Shared.Abstractions duplicates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Common.Services;
using PlantSafety.Domain.Intelligence.FeatureEngineering.Entities;
using PlantSafety.Domain.Intelligence.FeatureEngineering.Rules;
using PlantSafety.Domain.Intelligence.FeatureEngineering.ValueObjects;

namespace PlantSafety.Domain.Intelligence.FeatureEngineering.Services;

/// <summary>Stateless domain service for feature engineering business rules. No I/O permitted.</summary>
public sealed class FeatureEngineeringDomainService : IDomainService
{
    /// <summary>Returns <see langword="true"/> when <paramref name="candidate"/> is a newer version than <paramref name="existing"/> (higher major, then minor).</summary>
    public bool Supersedes(FeatureVersion candidate, FeatureVersion existing)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(existing);
        return candidate.Supersedes(existing);
    }

    /// <summary>Returns <see langword="true"/> when <paramref name="featureSet"/> is not yet published and contains at least one feature.</summary>
    public bool IsReadyToPublish(FeatureSet featureSet)
    {
        ArgumentNullException.ThrowIfNull(featureSet);
        return FeatureSetPublishRules.CanPublish(featureSet.IsPublished, featureSet.Features.Count);
    }
}
