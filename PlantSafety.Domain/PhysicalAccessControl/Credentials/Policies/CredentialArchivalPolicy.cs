// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.Credentials
//  Layer:          Domain
//  File:           CredentialArchivalPolicy.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for Credential aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.Credentials.Policies;

/// <summary>Determines whether a <see cref="Credential"/> aggregate is eligible for archival.</summary>
public static class CredentialArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the credential record may be archived (revoked or expired only).</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.PhysicalAccessControl.Credentials.Aggregates.Credential aggregate)
        => aggregate.Status is CredentialStatus.Revoked or CredentialStatus.Expired;
}
