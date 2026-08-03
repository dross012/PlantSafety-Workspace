// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tenancy.Tenant
//  Layer:          Domain
//  File:           ITenantRepository.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §2.1 Automated Tenant Creation)
//  SchemaVersion:  2026.1
//  Description:    Write-side repository abstraction for the Tenant aggregate. NOT itself
//                  tenant-scoped by a caller-supplied TenantId parameter (there is no "other"
//                  tenant to scope against -- a Tenant row's own identity IS the tenant), so
//                  lookups are by TenantId or Name alone, and listing is a platform-admin-only
//                  cross-tenant operation. See docs/Phase7/tenant-isolation-model.md for why the
//                  Tenancy.Tenant table itself is excluded from the SQL Row-Level-Security policy
//                  applied to every other tenant-scoped table. Additive-only evolution.
// ============================================================================
using TenantId = PlantSafety.Domain.Common.Types.StrongIds.TenantId;

namespace PlantSafety.Domain.Tenancy.Tenant.Interfaces;

/// <summary>Write-side repository abstraction for the Tenant aggregate.</summary>
public interface ITenantRepository
{
    /// <summary>Retrieves a Tenant by identity, or <see langword="null"/> if none exists.</summary>
    Task<Aggregates.Tenant?> GetByIdAsync(TenantId id, CancellationToken cancellationToken = default);

    /// <summary>Retrieves a Tenant by its registered name (case-insensitive), or <see langword="null"/> if none exists.</summary>
    Task<Aggregates.Tenant?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>Returns <see langword="true"/> when a non-deleted Tenant with the given name already exists.</summary>
    Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>Persists a newly provisioned Tenant.</summary>
    Task AddAsync(Aggregates.Tenant tenant, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing Tenant (lifecycle transitions, configuration updates).</summary>
    Task UpdateAsync(Aggregates.Tenant tenant, CancellationToken cancellationToken = default);

    /// <summary>Lists every registered Tenant, most recently created first. Platform-admin-only, cross-tenant operation.</summary>
    Task<IReadOnlyList<Aggregates.Tenant>> ListAllAsync(CancellationToken cancellationToken = default);
}
