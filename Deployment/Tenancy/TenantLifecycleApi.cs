// ============================================================================
//  Project:        Deployment
//  Module:         Tenancy
//  File:           TenantLifecycleApi.cs
//  Created:        2026-08-10
//  Description:    Phase 18 deliverable. The tenant lifecycle API contract:
//                  CreateTenant, UpdateTenant, SuspendTenant, DeleteTenant.
//                  Every method takes the real Shared.Authorization
//                  CallerContext (Phase 9/10, unchanged) so implementations
//                  can enforce authorization/MFA exactly the way every other
//                  module's *Authorization.cs already does -- this contract
//                  does not invent a parallel identity model.
// ============================================================================
using Shared.Authorization.Abstractions.Caller;

using Deployment.ConfigSchema.Contracts;

namespace Deployment.Tenancy;

/// <summary>The request to create a new tenant.</summary>
public sealed record CreateTenantRequest(
    string TenantId,
    string TenantName,
    TenantTopology Topology,
    TenantResourceQuota ResourceQuota,
    IReadOnlyList<string> AssignedModuleIds,
    IReadOnlyList<string> AssignedRoleIds);

/// <summary>The request to update an existing, active tenant's name and/or resource quota.</summary>
public sealed record UpdateTenantRequest(
    string TenantId,
    string? TenantName,
    TenantResourceQuota? ResourceQuota);

/// <summary>The request to suspend an existing, active tenant.</summary>
public sealed record SuspendTenantRequest(string TenantId, string Reason);

/// <summary>The request to permanently delete a tenant. Terminal -- a deleted tenant cannot be reactivated.</summary>
public sealed record DeleteTenantRequest(string TenantId, string Reason);

/// <summary>The deterministic outcome of a single tenant lifecycle operation.</summary>
public sealed record TenantLifecycleResult(TenantConfig? Tenant, bool Succeeded, string Reason);

/// <summary>The tenant lifecycle API: CreateTenant, UpdateTenant, SuspendTenant, DeleteTenant.</summary>
public interface ITenantLifecycleApi
{
    /// <summary>Creates a new, Active tenant. Fails if a tenant with the same TenantId already exists.</summary>
    Task<TenantLifecycleResult> CreateTenantAsync(CreateTenantRequest request, CallerContext caller, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing, Active tenant's name and/or resource quota. Fails if the tenant does not exist or is not Active.</summary>
    Task<TenantLifecycleResult> UpdateTenantAsync(UpdateTenantRequest request, CallerContext caller, CancellationToken cancellationToken = default);

    /// <summary>Suspends an existing, Active tenant. Fails if the tenant does not exist or is already Suspended/Deleted.</summary>
    Task<TenantLifecycleResult> SuspendTenantAsync(SuspendTenantRequest request, CallerContext caller, CancellationToken cancellationToken = default);

    /// <summary>Permanently deletes a tenant (terminal). Fails if the tenant does not exist or is already Deleted.</summary>
    Task<TenantLifecycleResult> DeleteTenantAsync(DeleteTenantRequest request, CallerContext caller, CancellationToken cancellationToken = default);
}
