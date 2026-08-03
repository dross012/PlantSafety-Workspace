// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MultiRegion.DataResidency
//  Layer:          Domain
//  File:           DataResidencyPolicy.cs
//  Created:        2026-08-03
//  Author:         Security & Compliance Agent (Phase 7 §5.3)
//  SchemaVersion:  2026.1
//  Description:    Phase 7 §5.3 (Data Residency) Smart Enum modeling a tenant's data-residency
//                  requirement: US-only, EU-only, or Global (no residency constraint). This is
//                  the "DataResidencyPolicy as a property on the Tenant aggregate" construct the
//                  spec calls for.
//
//                  INTEGRATION POINT FOR THE TENANT AGGREGATE: as of this pass, the concurrent
//                  Tenancy agent's `PlantSafety.Domain.Tenancy.Tenant` aggregate has landed
//                  structurally (Entities/Enums/Events/Exceptions/Rules/ValueObjects exist under
//                  `PlantSafety.Domain\Tenancy\Tenant\`) but has no `Aggregates\Tenant.cs` file yet
//                  and does not yet expose a data-residency concept. Per the same coordination
//                  pattern already established by `ITenantRegionRouter`'s file header (Application/
//                  Enterprise/MultiRegion), this type is therefore built standalone (keyed by a
//                  bare `Guid TenantId` via `TenantDataResidencyProfile` below) rather than as a
//                  property directly on an aggregate that does not yet exist in a buildable form.
//                  Once `Tenant` lands with its own aggregate root, the recommended migration path
//                  is: (1) add a `DataResidencyPolicy DataResidencyPolicy` property directly to
//                  `Tenant` initialized from this same Smart Enum, (2) have
//                  `TenantDataResidencyProfileStore`/`IDataResidencyEnforcer` (Application layer)
//                  read `Tenant.DataResidencyPolicy` as the source of truth instead of the
//                  standalone profile lookup, falling back to the standalone profile only for
//                  tenants that predate the aggregate's residency property. Additive-only
//                  evolution: new policy values may be appended; existing values must never change
//                  meaning.
// ============================================================================
namespace PlantSafety.Domain.MultiRegion.DataResidency;

/// <summary>
/// Smart Enum representing a tenant's data-residency requirement: the set of
/// <see cref="SharedKernel.Contracts.Enterprise.MultiRegion.Region"/> data-residency zones the
/// tenant's data is contractually/legally permitted to live in.
/// </summary>
public sealed class DataResidencyPolicy
{
    /// <summary>Stable numeric identifier (1-based).</summary>
    public int Id { get; }

    /// <summary>Stable machine-readable code (PascalCase).</summary>
    public string Code { get; }

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; }

    /// <summary>
    /// The single <see cref="SharedKernel.Contracts.Enterprise.MultiRegion.Region.DataResidencyZone"/>
    /// value this policy requires, or <see langword="null"/> for <see cref="Global"/> (no
    /// residency constraint -- any zone is permitted).
    /// </summary>
    public string? RequiredZone { get; }

    private DataResidencyPolicy(int id, string code, string displayName, string? requiredZone)
    {
        Id = id;
        Code = code;
        DisplayName = displayName;
        RequiredZone = requiredZone;
    }

    /// <summary>The tenant's data must reside only in a US-zone region.</summary>
    public static readonly DataResidencyPolicy UsOnly = new(1, "UsOnly", "US Only", "US");

    /// <summary>The tenant's data must reside only in the EU-zone region.</summary>
    public static readonly DataResidencyPolicy EuOnly = new(2, "EuOnly", "EU Only", "EU");

    /// <summary>No contractual/legal residency constraint -- the tenant's data may reside in any deployment region.</summary>
    public static readonly DataResidencyPolicy Global = new(3, "Global", "Global (No Residency Constraint)", requiredZone: null);

    /// <summary>Default policy for tenants with no explicit residency requirement configured.</summary>
    public static readonly DataResidencyPolicy Default = Global;

    /// <summary>All defined policies, in canonical (Id) order.</summary>
    public static readonly IReadOnlyList<DataResidencyPolicy> All = [UsOnly, EuOnly, Global];

    /// <summary>Resolves a <see cref="DataResidencyPolicy"/> from its <see cref="Code"/>. Throws if not found.</summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="code"/> does not match a known policy.</exception>
    public static DataResidencyPolicy From(string code)
        => TryParse(code, out var value)
            ? value!
            : throw new ArgumentException($"'{code}' is not a valid DataResidencyPolicy code.", nameof(code));

    /// <summary>Attempts to resolve a <see cref="DataResidencyPolicy"/> from its <see cref="Code"/>.</summary>
    public static bool TryParse(string? code, out DataResidencyPolicy? value)
    {
        value = All.FirstOrDefault(p => string.Equals(p.Code, code, StringComparison.OrdinalIgnoreCase));
        return value is not null;
    }

    /// <summary>
    /// Returns <see langword="true"/> when a deployment located in <paramref name="regionDataResidencyZone"/>
    /// (see <see cref="SharedKernel.Contracts.Enterprise.MultiRegion.Region.DataResidencyZone"/>)
    /// satisfies this policy. <see cref="Global"/> is always satisfied.
    /// </summary>
    public bool IsSatisfiedByZone(string regionDataResidencyZone)
        => RequiredZone is null || string.Equals(RequiredZone, regionDataResidencyZone, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override string ToString() => Code;
}
