// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Enterprise.MultiRegion
//  Layer:          SharedKernel
//  File:           Region.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Phase 7 §4.1 (Global Scalability & Multi-Region Deployment) Smart Enum
//                  enumerating the four deployment regions the SaaS Productization Blueprint
//                  targets: US East, US West, EU, and APAC. Carries a stable Id/Code pair plus
//                  the data-residency zone the region belongs to (used by data-sovereignty and
//                  compliance logic elsewhere in the platform) and its designated DR failover
//                  partner region.
//
//                  Lives in SharedKernel (not Domain) because it is a cross-cutting,
//                  infrastructure-adjacent concept referenced from Domain (failover policy),
//                  Application (tenant-region routing), and Infrastructure (health checks,
//                  RPO/RTO configuration) alike -- no single layer owns it exclusively.
//
//                  HONESTY NOTE: this type models region IDENTITY and static metadata only. No
//                  actual multi-region cloud infrastructure (real US East/US West/EU/APAC
//                  deployments, a real global load balancer, real geo-replication) exists in
//                  this sandboxed dev environment -- see docs\Phase7\multi-region-architecture.md
//                  for exactly what real infrastructure would be required to make this more than
//                  a domain model. Additive-only evolution: new regions may be appended; existing
//                  Id/Code/DisplayName values must never change meaning.
// ============================================================================
namespace PlantSafety.SharedKernel.Contracts.Enterprise.MultiRegion;

/// <summary>
/// Smart Enum representing one of the platform's four target deployment regions. Carries
/// stable identity plus the metadata (data-residency zone, DR failover partner) that downstream
/// domain logic (tenant-region routing, failover policy, RPO/RTO targets) depends on.
/// </summary>
public sealed class Region
{
    /// <summary>Stable numeric identifier (1-based).</summary>
    public int Id { get; }

    /// <summary>Stable machine-readable code (PascalCase, no abbreviations beyond the region's own common short form).</summary>
    public string Code { get; }

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; }

    /// <summary>
    /// The data-residency zone this region belongs to (e.g. "US", "EU", "APAC"). Used by
    /// data-sovereignty/compliance logic that must keep tenant data within a legal jurisdiction.
    /// </summary>
    public string DataResidencyZone { get; }

    private Region(int id, string code, string displayName, string dataResidencyZone)
    {
        Id = id;
        Code = code;
        DisplayName = displayName;
        DataResidencyZone = dataResidencyZone;
    }

    /// <summary>US East region (primary region for North American tenants).</summary>
    public static readonly Region UsEast = new(1, "UsEast", "US East", "US");

    /// <summary>US West region (secondary/DR-paired region for North American tenants).</summary>
    public static readonly Region UsWest = new(2, "UsWest", "US West", "US");

    /// <summary>EU region (primary region for European tenants; data-residency-bound to the EU zone).</summary>
    public static readonly Region Eu = new(3, "Eu", "European Union", "EU");

    /// <summary>APAC region (primary region for Asia-Pacific tenants).</summary>
    public static readonly Region Apac = new(4, "Apac", "Asia-Pacific", "APAC");

    /// <summary>Default region used when no explicit tenant-region affinity has been assigned.</summary>
    public static readonly Region Default = UsEast;

    /// <summary>All defined regions, in canonical (Id) order.</summary>
    public static readonly IReadOnlyList<Region> All =
    [
        UsEast,
        UsWest,
        Eu,
        Apac,
    ];

    /// <summary>Resolves a <see cref="Region"/> from its <see cref="Code"/>. Throws if not found.</summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="code"/> does not match a known region.</exception>
    public static Region From(string code)
        => TryParse(code, out var value)
            ? value!
            : throw new ArgumentException($"'{code}' is not a valid Region code.", nameof(code));

    /// <summary>Attempts to resolve a <see cref="Region"/> from its <see cref="Code"/>.</summary>
    public static bool TryParse(string? code, out Region? value)
    {
        value = All.FirstOrDefault(r => string.Equals(r.Code, code, StringComparison.OrdinalIgnoreCase));
        return value is not null;
    }

    /// <summary>Returns <see langword="true"/> when <paramref name="code"/> is a recognised region code.</summary>
    public static bool IsValid(string? code) => TryParse(code, out _);

    /// <inheritdoc/>
    public override string ToString() => Code;
}
