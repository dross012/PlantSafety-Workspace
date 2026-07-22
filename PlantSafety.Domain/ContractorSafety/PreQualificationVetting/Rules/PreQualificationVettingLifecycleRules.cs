// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.PreQualificationVetting
//  Layer:          Domain
//  File:           PreQualificationVettingLifecycleRules.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Defines permitted lifecycle transitions for the PreQualificationVetting aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.PreQualificationVetting.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="PreQualificationVetting"/> aggregate.</summary>
public static class PreQualificationVettingLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when review may begin from <paramref name="current"/>.</summary>
    public static bool CanBeginReview(PreQualificationVettingStatus current) =>
        current is PreQualificationVettingStatus.SubmittedForReview or PreQualificationVettingStatus.Expired;

    /// <summary>Returns <see langword="true"/> when the contractor may be approved from <paramref name="current"/>.</summary>
    public static bool CanApprove(PreQualificationVettingStatus current) => current == PreQualificationVettingStatus.UnderReview;

    /// <summary>Returns <see langword="true"/> when the contractor may be rejected from <paramref name="current"/>.</summary>
    public static bool CanReject(PreQualificationVettingStatus current) => current == PreQualificationVettingStatus.UnderReview;

    /// <summary>Returns <see langword="true"/> when the approval may expire from <paramref name="current"/>.</summary>
    public static bool CanExpire(PreQualificationVettingStatus current) => current == PreQualificationVettingStatus.Approved;
}
