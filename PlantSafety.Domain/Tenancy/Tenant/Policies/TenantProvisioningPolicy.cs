// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tenancy.Tenant
//  Layer:          Domain
//  File:           TenantProvisioningPolicy.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §2.1 Automated Tenant Creation)
//  SchemaVersion:  2026.1
//  Description:    Domain policy encapsulating the invariants a new Tenant registration must
//                  satisfy before the automated provisioning pipeline may begin (name uniqueness,
//                  supported region, well-formed name). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Tenancy.Tenant.Policies;

/// <summary>Encapsulates the invariants a new Tenant registration must satisfy before provisioning begins.</summary>
public static class TenantProvisioningPolicy
{
    /// <summary>Deployment regions this platform currently provisions tenants into (Global-Scale Multi-Region architecture).</summary>
    public static readonly IReadOnlyList<string> SupportedRegions = ["us-east", "us-west", "eu-west", "ap-southeast"];

    /// <summary>Maximum allowed length for a Tenant's registered name.</summary>
    public const int MaxNameLength = 200;

    /// <summary>Returns <see langword="true"/> when <paramref name="region"/> is one of <see cref="SupportedRegions"/> (case-insensitive).</summary>
    public static bool IsSupportedRegion(string region) =>
        !string.IsNullOrWhiteSpace(region) &&
        SupportedRegions.Any(r => string.Equals(r, region, StringComparison.OrdinalIgnoreCase));

    /// <summary>Returns <see langword="true"/> when <paramref name="name"/> is a well-formed Tenant name (non-empty, within <see cref="MaxNameLength"/>).</summary>
    public static bool IsWellFormedName(string name) =>
        !string.IsNullOrWhiteSpace(name) && name.Trim().Length <= MaxNameLength;
}
