// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessPoints
//  Layer:          Domain
//  File:           AccessPointArchivalPolicy.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for AccessPoint aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Policies;

/// <summary>Determines whether an <see cref="AccessPoint"/> aggregate is eligible for archival.</summary>
public static class AccessPointArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the access point record may be archived (decommissioned only).</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Aggregates.AccessPoint aggregate)
        => aggregate.Status == AccessPointStatus.Decommissioned;
}
