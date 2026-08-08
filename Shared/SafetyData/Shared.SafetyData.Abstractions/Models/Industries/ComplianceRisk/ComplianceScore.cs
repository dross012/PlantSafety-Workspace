// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.ComplianceRisk
//  Layer:          Abstractions
//  File:           ComplianceScore.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    A facility's compliance score, re-framed from its own
//                  FacilityComplianceProfile (Facilities/FacilityComplianceProfile.cs)
//                  into a tiered classification with explanatory notes. Never
//                  recomputes RequiredElements/CompletedElements/
//                  MissingElements/PercentComplete independently -- those are
//                  read directly off the facility's own
//                  FacilityProfile.ComplianceProfile, which remains the
//                  single source of truth for what is required/complete/
//                  missing. This type adds only two things
//                  FacilityComplianceProfile does not already have:
//                  ComplianceTier (a coarser, audience-facing classification)
//                  and human-readable Notes. Additive-only evolution.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries.Facilities;

namespace Shared.SafetyData.Abstractions.Models.Industries.ComplianceRisk;

/// <summary>A coarse, audience-facing compliance classification derived from <see cref="ComplianceScore.PercentComplete"/>.</summary>
public enum ComplianceTier
{
    /// <summary>0% of required elements complete.</summary>
    NonCompliant,

    /// <summary>More than 0% but less than 75% of required elements complete.</summary>
    PartiallyCompliant,

    /// <summary>At least 75% but less than 100% of required elements complete.</summary>
    SubstantiallyCompliant,

    /// <summary>100% of required elements complete.</summary>
    FullyCompliant,
}

/// <summary>
/// A single facility's compliance score: the same Required/Completed/Missing counts as its
/// <see cref="FacilityComplianceProfile"/>, re-framed into a <see cref="ComplianceTier"/> with
/// explanatory notes.
/// </summary>
/// <param name="FacilityId">The facility this score was computed for.</param>
/// <param name="RequiredElementsCount">Count of required compliance elements (<see cref="FacilityComplianceProfile.RequiredElements"/>).</param>
/// <param name="CompletedElementsCount">Count of completed compliance elements (<see cref="FacilityComplianceProfile.CompletedElements"/>).</param>
/// <param name="MissingElementsCount">Count of missing compliance elements (<see cref="FacilityComplianceProfile.MissingElements"/>).</param>
/// <param name="PercentComplete">The facility's compliance percentage (<see cref="FacilityComplianceProfile.ComplianceScorePercent"/>).</param>
/// <param name="ComplianceTier">The coarse tier <paramref name="PercentComplete"/> falls into.</param>
/// <param name="Notes">A short, deterministic, human-readable explanation of the score.</param>
public sealed record ComplianceScore(
    Guid FacilityId,
    int RequiredElementsCount,
    int CompletedElementsCount,
    int MissingElementsCount,
    double PercentComplete,
    ComplianceTier ComplianceTier,
    string Notes);

/// <summary>Builds <see cref="ComplianceScore"/> instances deterministically from a <see cref="FacilityProfile"/>.</summary>
public static class ComplianceScoreBuilder
{
    /// <summary>Builds the compliance score for the given facility.</summary>
    public static ComplianceScore Build(FacilityProfile facility)
    {
        FacilityComplianceProfile profile = facility.ComplianceProfile;
        ComplianceTier tier = TierFor(profile.ComplianceScorePercent);

        return new ComplianceScore(
            FacilityId: facility.FacilityId,
            RequiredElementsCount: profile.RequiredElements.Count,
            CompletedElementsCount: profile.CompletedElements.Count,
            MissingElementsCount: profile.MissingElements.Count,
            PercentComplete: profile.ComplianceScorePercent,
            ComplianceTier: tier,
            Notes: NotesFor(profile, tier));
    }

    /// <summary>Derives the <see cref="ComplianceTier"/> for a given compliance percentage.</summary>
    public static ComplianceTier TierFor(double percentComplete) => percentComplete switch
    {
        >= 100d => ComplianceTier.FullyCompliant,
        >= 75d => ComplianceTier.SubstantiallyCompliant,
        > 0d => ComplianceTier.PartiallyCompliant,
        _ => ComplianceTier.NonCompliant,
    };

    private static string NotesFor(FacilityComplianceProfile profile, ComplianceTier tier) => tier switch
    {
        ComplianceTier.FullyCompliant => "All required compliance elements are complete.",
        ComplianceTier.SubstantiallyCompliant => $"{profile.MissingElements.Count} required element(s) remain incomplete; facility is substantially compliant.",
        ComplianceTier.PartiallyCompliant => $"{profile.MissingElements.Count} of {profile.RequiredElements.Count} required elements are incomplete; facility is partially compliant.",
        _ => "No required compliance elements have been completed.",
    };
}
