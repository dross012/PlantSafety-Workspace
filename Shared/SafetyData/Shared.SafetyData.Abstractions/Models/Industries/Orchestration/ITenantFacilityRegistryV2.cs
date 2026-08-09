// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.Orchestration
//  Layer:          Abstractions
//  File:           ITenantFacilityRegistryV2.cs
//  Created:        2026-08-08
//  Description:    Phase C.8: multi-facility-per-tenant registry supporting
//                  facility lookup, routing (access checks), and dashboard
//                  ordering (primary facility first). Parallel to, not a
//                  replacement for, ITenantFacilityRegistry (Phase 3,
//                  single-facility-per-tenant).
// ============================================================================
namespace Shared.SafetyData.Abstractions.Models.Industries.Orchestration;

/// <summary>Resolves a PlantSafety TenantId to every facility it is bound to, supporting lookup, routing, and dashboard ordering.</summary>
public interface ITenantFacilityRegistryV2
{
    /// <summary>
    /// Binds a facility to a tenant. If <paramref name="isPrimary"/> is true, or the tenant currently has no
    /// primary facility, this facility becomes the tenant's primary. Re-registering an already-bound facility
    /// is a no-op beyond any primary-designation change.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="tenantId"/> is null or whitespace, or <paramref name="facilityId"/> is <see cref="Guid.Empty"/>.</exception>
    void RegisterFacility(string tenantId, Guid facilityId, bool isPrimary = false);

    /// <summary>
    /// Unbinds a facility from a tenant. If the removed facility was primary, the earliest-registered
    /// remaining facility (if any) is deterministically promoted to primary. Returns whether a binding was removed.
    /// </summary>
    bool UnregisterFacility(string tenantId, Guid facilityId);

    /// <summary>Attempts to look up every facility bound to the given tenant.</summary>
    bool TryGetFacilities(string tenantId, out IReadOnlyList<Guid> facilityIds);

    /// <summary>Attempts to resolve the tenant's primary facility.</summary>
    bool TryResolvePrimaryFacility(string tenantId, out Guid facilityId);

    /// <summary>True if the given facility is bound to the given tenant -- an access/routing check before serving tenant-scoped facility data.</summary>
    bool CanRouteToFacility(string tenantId, Guid facilityId);

    /// <summary>Returns the tenant's facilities ordered for dashboard display: the primary facility first (if any), then the rest in registration order.</summary>
    IReadOnlyList<Guid> GetDashboardFacilityIds(string tenantId);

    /// <summary>Every currently registered tenant/facility-map, in no particular order.</summary>
    IReadOnlyList<TenantFacilityMap> GetAll();
}
