// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.Manifests
//  Layer:          Domain
//  File:           ManifestRetentionPolicy.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain policy encoding the manifest data-governance invariants: minimum
//                  3-year retention and immutability once transporter-signed.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.Manifests.Aggregates;

namespace PlantSafety.Domain.HazardousWasteManagement.Manifests.Policies;

/// <summary>Encodes manifest data-governance invariants: retention and immutability.</summary>
public static class ManifestRetentionPolicy
{
    /// <summary>Returns <see langword="true"/> when the manifest's mandatory 3-year retention window has expired and it is eligible for archival (never physical deletion).</summary>
    public static bool IsEligibleForArchival(HazardousWasteManifest manifest, DateTime nowUtc)
        => manifest.RetentionExpiresAtUtc.Value <= nowUtc;

    /// <summary>Returns <see langword="true"/> when the manifest may still be mutated (not yet transporter-signed).</summary>
    public static bool IsMutable(HazardousWasteManifest manifest) => !manifest.IsImmutable;
}
