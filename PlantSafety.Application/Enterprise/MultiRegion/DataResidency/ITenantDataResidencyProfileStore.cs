// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Application.Enterprise.MultiRegion.DataResidency
//  Layer:          Application
//  File:           ITenantDataResidencyProfileStore.cs
//  Created:        2026-08-03
//  Author:         Security & Compliance Agent (Phase 7 §5.3)
//  SchemaVersion:  2026.1
//  Description:    Abstraction over "what is this tenant's configured data-residency
//                  requirement". Standalone store (not the Tenant aggregate) -- see
//                  DataResidencyPolicy's file header for the Tenant-aggregate integration point
//                  this is standing in for. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.MultiRegion.DataResidency;

namespace PlantSafety.Application.Enterprise.MultiRegion.DataResidency;

/// <summary>Resolves a tenant's configured <see cref="TenantDataResidencyProfile"/>.</summary>
public interface ITenantDataResidencyProfileStore
{
    /// <summary>
    /// Resolves <paramref name="tenantId"/>'s configured data-residency profile. Tenants with no
    /// explicit configuration resolve to <see cref="TenantDataResidencyProfile.Unrestricted"/>.
    /// </summary>
    TenantDataResidencyProfile GetProfile(Guid tenantId);
}
