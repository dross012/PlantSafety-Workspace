// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.PreQualificationVetting
//  Layer:          Domain
//  File:           PreQualificationVettingArchivalPolicy.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain policy governing archival eligibility for PreQualificationVetting aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.PreQualificationVetting.Policies;

/// <summary>Determines whether a <see cref="PreQualificationVetting"/> aggregate is eligible for archival.</summary>
public static class PreQualificationVettingArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the vetting record may be archived (rejected or expired).</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.ContractorSafety.PreQualificationVetting.Aggregates.PreQualificationVetting aggregate)
        => aggregate.Status is PreQualificationVettingStatus.Rejected or PreQualificationVettingStatus.Expired;
}
