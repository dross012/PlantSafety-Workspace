// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.AgencyDirectory
//  Layer:          Domain
//  File:           PublicSafetyAgencyArchivalPolicy.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for PublicSafetyAgency aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Policies;

/// <summary>Determines whether a <see cref="PublicSafetyAgency"/> aggregate is eligible for archival.</summary>
public static class PublicSafetyAgencyArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the agency record may be archived (inactive only).</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Aggregates.PublicSafetyAgency aggregate)
        => aggregate.Status == AgencyOperationalStatus.Inactive;
}
