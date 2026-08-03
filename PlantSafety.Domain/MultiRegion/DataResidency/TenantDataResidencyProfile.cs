// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MultiRegion.DataResidency
//  Layer:          Domain
//  File:           TenantDataResidencyProfile.cs
//  Created:        2026-08-03
//  Author:         Security & Compliance Agent (Phase 7 §5.3)
//  SchemaVersion:  2026.1
//  Description:    Standalone pairing of a bare TenantId with its DataResidencyPolicy -- stands
//                  in for "DataResidencyPolicy as a property on the Tenant aggregate" until the
//                  concurrent Tenancy agent's Tenant aggregate exposes the property directly (see
//                  DataResidencyPolicy's file header for the full integration-point note).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MultiRegion.DataResidency;

/// <summary>
/// Pairs a tenant identifier with its configured <see cref="DataResidencyPolicy"/>. Immutable,
/// persistence-agnostic value object.
/// </summary>
/// <param name="TenantId">The tenant this profile describes.</param>
/// <param name="Policy">The tenant's configured data-residency requirement.</param>
public sealed record TenantDataResidencyProfile(Guid TenantId, DataResidencyPolicy Policy)
{
    /// <summary>Creates the default profile (<see cref="DataResidencyPolicy.Default"/>) for a tenant with no explicit residency requirement configured.</summary>
    public static TenantDataResidencyProfile Unrestricted(Guid tenantId) => new(tenantId, DataResidencyPolicy.Default);
}
