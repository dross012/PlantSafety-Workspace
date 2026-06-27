/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               FeatureEngineeringDomainService.cs
    SCHEMA VERSION:     2026.1  |  EVOLUTION: Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.FeatureEngineering.Entities;
using Intelligence.Domain.FeatureEngineering.ValueObjects;

namespace Intelligence.Domain.FeatureEngineering.Services;

/// <summary>Stateless domain service for feature engineering business rules. No I/O permitted.</summary>
public sealed class FeatureEngineeringDomainService
{
    /// <summary>Returns true when candidate is a newer version than existing (higher major, then minor).</summary>
    public bool Supersedes(FeatureVersion candidate, FeatureVersion existing)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(existing);
        if (candidate.Major != existing.Major) return candidate.Major > existing.Major;
        return candidate.Minor > existing.Minor;
    }

    /// <summary>Returns true when a feature set is not yet published and contains at least one feature.</summary>
    public bool IsReadyToPublish(FeatureSet featureSet)
    {
        ArgumentNullException.ThrowIfNull(featureSet);
        return !featureSet.IsPublished && featureSet.Features.Count > 0;
    }
}
