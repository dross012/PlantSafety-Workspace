// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Application.Enterprise.MultiRegion.DataResidency
//  Layer:          Application
//  File:           TenantDataResidencyOptions.cs
//  Created:        2026-08-03
//  Author:         Security & Compliance Agent (Phase 7 §5.3)
//  SchemaVersion:  2026.1
//  Description:    Configuration-backed store of explicit tenant data-residency requirements,
//                  mirroring TenantRegionAffinityOptions' shape/rationale exactly (both are
//                  interim, config-driven stand-ins for a property that will eventually live on
//                  the Tenant aggregate). Bound from configuration section
//                  "MultiRegion:TenantDataResidency". Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Enterprise.MultiRegion.DataResidency;

/// <summary>Strongly typed configuration for explicit tenant data-residency requirements.</summary>
public sealed class TenantDataResidencyOptions
{
    /// <summary>Configuration section key.</summary>
    public const string SectionName = "MultiRegion:TenantDataResidency";

    /// <summary>
    /// Explicit tenant data-residency requirements, keyed by tenant ID (string form, see
    /// <see cref="TenantRegionAffinityOptions.Overrides"/> for the identical binding rationale),
    /// with the required <see cref="Domain.MultiRegion.DataResidency.DataResidencyPolicy.Code"/>
    /// as the value (e.g. <c>"EuOnly"</c>). Tenants not present in this dictionary resolve to
    /// <see cref="Domain.MultiRegion.DataResidency.DataResidencyPolicy.Default"/>.
    /// </summary>
    public IReadOnlyDictionary<string, string> Policies { get; init; } =
        new Dictionary<string, string>();
}
