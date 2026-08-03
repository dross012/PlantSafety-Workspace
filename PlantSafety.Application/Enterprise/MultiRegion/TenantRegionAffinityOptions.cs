// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Application.Enterprise.MultiRegion
//  Layer:          Application
//  File:           TenantRegionAffinityOptions.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Phase 7 §4.3 (Global Routing) configuration for explicit tenant-to-region
//                  overrides, consumed by TenantRegionAffinityResolver. Until the Tenant
//                  aggregate lands with its own region-affinity data (see ITenantRegionRouter's
//                  file header), this is the only mechanism to pin a specific tenant to a
//                  specific region (e.g. for an EU tenant with a contractual data-residency
//                  requirement) ahead of the deterministic hash-based fallback assignment.
//                  Bound from configuration section "MultiRegion:TenantAffinity". Additive-only
//                  evolution.
// ============================================================================
namespace PlantSafety.Application.Enterprise.MultiRegion;

/// <summary>
/// Strongly typed configuration for explicit tenant-to-region overrides.
/// </summary>
public sealed class TenantRegionAffinityOptions
{
    /// <summary>Configuration section key.</summary>
    public const string SectionName = "MultiRegion:TenantAffinity";

    /// <summary>
    /// Explicit tenant-to-region overrides, keyed by tenant ID (string form, since
    /// <see cref="IReadOnlyDictionary{TKey,TValue}"/> configuration binding requires string
    /// keys), with the target region's <c>Code</c> as the value (e.g. <c>"Eu"</c>). Tenants not
    /// present in this dictionary fall back to the deterministic hash-based assignment.
    /// </summary>
    public IReadOnlyDictionary<string, string> Overrides { get; init; } =
        new Dictionary<string, string>();
}
