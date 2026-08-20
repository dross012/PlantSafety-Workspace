// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.ModelRegistry
//  Layer:          Domain
//  File:           ModelRegistryDomainService.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    SAFETY-CRITICAL domain service: enforces production deployment eligibility
//                  (Safety Rule 4.5) and SemVer-based deployment supersession for
//                  ModelRegistryEntry. Ported (rebuilt, preserving the exact business logic and
//                  fail-safe posture) from the orphaned, dead
//                  Intelligence.Domain/ModelRegistry/Services/ModelRegistryDomainService.cs onto
//                  this module's own ModelRegistryEntry aggregate/ModelVersion value object and
//                  PlantSafety.Domain.Common.Services.IDomainService marker interface (stateless --
//                  no I/O permitted per that interface's contract) instead of the orphan's local
//                  Intelligence.Domain.Shared.Abstractions duplicates. The
//                  IntelligenceConstants.MinimumConfidenceScore threshold referenced by the orphan
//                  is inlined here as MinimumConfidenceScore since no shared Intelligence-module
//                  constants holder exists among this module's sibling sub-folders (AIInvestigations
//                  / AITraining / DataLake / PredictiveSafety) to extend.
//                  Fail safe: deployment eligibility defaults to false for any uncertain state.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Common.Services;
using PlantSafety.Domain.Intelligence.ModelRegistry.Aggregates;
using PlantSafety.Domain.Intelligence.ModelRegistry.Rules;

namespace PlantSafety.Domain.Intelligence.ModelRegistry.Services;

/// <summary>
/// SAFETY-CRITICAL: Domain service for model registry governance rules. Stateless -- no I/O.
/// Fail safe: deployment eligibility defaults to false for any uncertain state (Safety Rule 4.5).
/// </summary>
public sealed class ModelRegistryDomainService : IDomainService
{
    /// <summary>
    /// SAFETY-CRITICAL (Safety Rule 4.5): Minimum training accuracy score required for an
    /// Approved model to be eligible for production deployment. May only be raised, never
    /// lowered.
    /// </summary>
    public const double MinimumConfidenceScore = 0.70;

    /// <summary>
    /// SAFETY-CRITICAL: Returns <see langword="true"/> only when <paramref name="entry"/> is
    /// Approved and meets the minimum training accuracy threshold. Fail safe (Safety Rule 4.5):
    /// returns <see langword="false"/> for all uncertain or non-approved states.
    /// </summary>
    public bool IsEligibleForProductionDeployment(ModelRegistryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        return ModelRegistryEntryLifecycleRules.IsDeployable(entry.Status)
               && entry.TrainingAccuracyScore >= MinimumConfidenceScore;
    }

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="candidate"/> supersedes
    /// <paramref name="current"/> for the same logical model name. Candidate must be deployable
    /// (Approved) and carry a strictly higher SemVer ordinal than current.
    /// </summary>
    public bool SupersedesCurrentDeployment(ModelRegistryEntry candidate, ModelRegistryEntry current)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(current);

        if (!ModelRegistryEntryLifecycleRules.IsDeployable(candidate.Status)) return false;
        if (!string.Equals(candidate.ModelName, current.ModelName, StringComparison.OrdinalIgnoreCase)) return false;

        return candidate.Version.IsGreaterThan(current.Version);
    }
}
