/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               ModelRegistryDomainService.cs
    DESCRIPTION:        SAFETY-CRITICAL domain service: enforces production deployment eligibility (Safety Rule 4.5).
    SCHEMA VERSION:     2026.1  |  EVOLUTION: Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.ModelRegistry.Entities;
using Intelligence.Domain.Shared.Constants;

namespace Intelligence.Domain.ModelRegistry.Services;

/// <summary>
/// SAFETY-CRITICAL: Domain service for model registry governance rules. Stateless — no I/O.
/// Fail safe: deployment eligibility defaults to false for any uncertain state.
/// </summary>
public sealed class ModelRegistryDomainService
{
    /// <summary>
    /// SAFETY-CRITICAL: Returns true only when the entry is Approved and meets the training accuracy threshold.
    /// Fail safe (Safety Rule 4.5): returns false for all uncertain or non-approved states.
    /// </summary>
    public bool IsEligibleForProductionDeployment(ModelRegistryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        return entry.Status.IsDeployable
               && entry.TrainingAccuracyScore >= IntelligenceConstants.MinimumConfidenceScore;
    }

    /// <summary>
    /// Returns true when candidate supersedes current for the same logical model name.
    /// Candidate must be Approved and have a higher SemVer ordinal.
    /// </summary>
    public bool SupersedesCurrentDeployment(ModelRegistryEntry candidate, ModelRegistryEntry current)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(current);

        if (!candidate.Status.IsDeployable) return false;
        if (!string.Equals(candidate.ModelName, current.ModelName, StringComparison.OrdinalIgnoreCase)) return false;

        if (candidate.Version.Major != current.Version.Major) return candidate.Version.Major > current.Version.Major;
        if (candidate.Version.Minor != current.Version.Minor) return candidate.Version.Minor > current.Version.Minor;
        return candidate.Version.Patch > current.Version.Patch;
    }
}
