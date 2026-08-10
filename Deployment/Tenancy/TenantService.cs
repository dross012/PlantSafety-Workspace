// ============================================================================
//  Project:        Deployment
//  Module:         Tenancy
//  File:           TenantService.cs
//  Created:        2026-08-10
//  Description:    Phase 18 deliverable. Deterministic, thread-safe,
//                  in-memory ITenantLifecycleApi implementation. Every
//                  mutating operation runs through the real Shared.
//                  Authorization.Abstractions.Checks.AuthorizationCheck
//                  (Phase 9, unchanged) for module "Deployment",
//                  ModulePermissionVerb.Admin -- the same tier every other
//                  module's *Authorization.cs already requires for its most
//                  sensitive operations, and the one verb AuthorizationCheck
//                  itself already gates on MfaStatus.Verified (Phase 9's own
//                  built-in MFA requirement), satisfying this phase's "MFA
//                  status in APIs" requirement without inventing a second
//                  MFA gate.
// ============================================================================
using System.Collections.Concurrent;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Checks;
using Shared.Authorization.Abstractions.Permissions;
using Shared.Deterministic.Abstractions.Clock;

using Deployment.ConfigSchema.Contracts;

namespace Deployment.Tenancy;

/// <summary>Deterministic, thread-safe, in-memory tenant lifecycle service.</summary>
public sealed class TenantService(IPermissionResolver permissionResolver, IClock clock) : ITenantLifecycleApi
{
    private const string Module = "Deployment";

    private readonly ConcurrentDictionary<string, TenantConfig> _tenants = new(StringComparer.Ordinal);

    /// <summary>Returns the current TenantConfig for <paramref name="tenantId"/>, or null if it does not exist.</summary>
    public TenantConfig? TryGetTenant(string tenantId)
    {
        ArgumentNullException.ThrowIfNull(tenantId);
        return _tenants.TryGetValue(tenantId, out TenantConfig? tenant) ? tenant : null;
    }

    /// <inheritdoc />
    public Task<TenantLifecycleResult> CreateTenantAsync(CreateTenantRequest request, CallerContext caller, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(caller);

        TenantLifecycleResult? denial = AuthorizeAdmin(caller);
        if (denial is not null)
        {
            return Task.FromResult(denial);
        }

        if (_tenants.ContainsKey(request.TenantId))
        {
            return Task.FromResult(new TenantLifecycleResult(null, false, $"Tenant \"{request.TenantId}\" already exists."));
        }

        var metadata = new DeploymentContractMetadata(DeploymentContractMetadata.CurrentVersion, request.TenantId, caller.CorrelationId, clock.UtcNow);
        var tenant = new TenantConfig(
            metadata, request.TenantId, request.TenantName, request.Topology, TenantStatus.Active,
            request.ResourceQuota, request.AssignedModuleIds, request.AssignedRoleIds);

        _tenants[request.TenantId] = tenant;
        return Task.FromResult(new TenantLifecycleResult(tenant, true, "Tenant created."));
    }

    /// <inheritdoc />
    public Task<TenantLifecycleResult> UpdateTenantAsync(UpdateTenantRequest request, CallerContext caller, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(caller);

        TenantLifecycleResult? denial = AuthorizeAdmin(caller);
        if (denial is not null)
        {
            return Task.FromResult(denial);
        }

        if (!_tenants.TryGetValue(request.TenantId, out TenantConfig? existing))
        {
            return Task.FromResult(new TenantLifecycleResult(null, false, $"Tenant \"{request.TenantId}\" does not exist."));
        }

        if (existing.Status != TenantStatus.Active)
        {
            return Task.FromResult(new TenantLifecycleResult(existing, false, $"Tenant \"{request.TenantId}\" is not Active (current status: {existing.Status})."));
        }

        var metadata = new DeploymentContractMetadata(DeploymentContractMetadata.CurrentVersion, request.TenantId, caller.CorrelationId, clock.UtcNow);
        var updated = existing with
        {
            Metadata = metadata,
            TenantName = request.TenantName ?? existing.TenantName,
            ResourceQuota = request.ResourceQuota ?? existing.ResourceQuota,
        };

        _tenants[request.TenantId] = updated;
        return Task.FromResult(new TenantLifecycleResult(updated, true, "Tenant updated."));
    }

    /// <inheritdoc />
    public Task<TenantLifecycleResult> SuspendTenantAsync(SuspendTenantRequest request, CallerContext caller, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(caller);

        TenantLifecycleResult? denial = AuthorizeAdmin(caller);
        if (denial is not null)
        {
            return Task.FromResult(denial);
        }

        if (!_tenants.TryGetValue(request.TenantId, out TenantConfig? existing))
        {
            return Task.FromResult(new TenantLifecycleResult(null, false, $"Tenant \"{request.TenantId}\" does not exist."));
        }

        if (existing.Status == TenantStatus.Deleted)
        {
            return Task.FromResult(new TenantLifecycleResult(existing, false, $"Tenant \"{request.TenantId}\" is Deleted and cannot be suspended."));
        }

        var metadata = new DeploymentContractMetadata(DeploymentContractMetadata.CurrentVersion, request.TenantId, caller.CorrelationId, clock.UtcNow);
        var suspended = existing with { Metadata = metadata, Status = TenantStatus.Suspended };

        _tenants[request.TenantId] = suspended;
        return Task.FromResult(new TenantLifecycleResult(suspended, true, $"Tenant suspended: {request.Reason}"));
    }

    /// <inheritdoc />
    public Task<TenantLifecycleResult> DeleteTenantAsync(DeleteTenantRequest request, CallerContext caller, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(caller);

        TenantLifecycleResult? denial = AuthorizeAdmin(caller);
        if (denial is not null)
        {
            return Task.FromResult(denial);
        }

        if (!_tenants.TryGetValue(request.TenantId, out TenantConfig? existing))
        {
            return Task.FromResult(new TenantLifecycleResult(null, false, $"Tenant \"{request.TenantId}\" does not exist."));
        }

        if (existing.Status == TenantStatus.Deleted)
        {
            return Task.FromResult(new TenantLifecycleResult(existing, false, $"Tenant \"{request.TenantId}\" is already Deleted."));
        }

        var metadata = new DeploymentContractMetadata(DeploymentContractMetadata.CurrentVersion, request.TenantId, caller.CorrelationId, clock.UtcNow);
        var deleted = existing with { Metadata = metadata, Status = TenantStatus.Deleted };

        _tenants[request.TenantId] = deleted;
        return Task.FromResult(new TenantLifecycleResult(deleted, true, $"Tenant deleted: {request.Reason}"));
    }

    private TenantLifecycleResult? AuthorizeAdmin(CallerContext caller)
    {
        AuthorizationDecision decision = AuthorizationCheck.Evaluate(caller, permissionResolver, Module, ModulePermissionVerb.Admin);
        return decision.IsAuthorized ? null : new TenantLifecycleResult(null, false, decision.Reason);
    }
}
