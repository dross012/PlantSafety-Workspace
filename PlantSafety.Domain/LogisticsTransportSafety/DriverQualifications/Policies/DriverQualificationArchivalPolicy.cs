// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.DriverQualifications
//  Layer:          Domain
//  File:           DriverQualificationArchivalPolicy.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for DriverQualification aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.Policies;

/// <summary>Determines whether a <see cref="DriverQualification"/> aggregate is eligible for archival.</summary>
public static class DriverQualificationArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the record may be archived (retired or expired only).</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.Aggregates.DriverQualification aggregate)
        => aggregate.Status is DriverQualificationStatus.Retired or DriverQualificationStatus.Expired;
}
