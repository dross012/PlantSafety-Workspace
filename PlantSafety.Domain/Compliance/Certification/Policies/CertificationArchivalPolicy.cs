// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Certification
//  Layer:          Domain
//  File:           CertificationArchivalPolicy.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for Certification aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Certification.Policies;

/// <summary>Determines whether a <see cref="Certification"/> aggregate is eligible for archival.</summary>
public static class CertificationArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the Certification may be archived.</summary>
    public static bool IsEligibleForArchival(Certification aggregate)
        => aggregate.Status == CertificationStatus.Closed;
}
