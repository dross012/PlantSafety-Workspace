// ============================================================================
//  Project:        Shared.Authorization
//  Module:         Abstractions.Tenant
//  File:           TenantContextProvider.cs
//  Created:        2026-08-10
//  Description:    Phase 11 deliverable. Concrete, scoped implementation of
//                  ITenantContextProvider -- structurally identical to
//                  CallerContextProvider (Phase 10).
// ============================================================================
namespace Shared.Authorization.Abstractions.Tenant;

/// <summary>Scoped, Set-once-then-read carrier for the current request's resolved TenantContext.</summary>
public sealed class TenantContextProvider : ITenantContextProvider
{
    private TenantContext? _current;

    /// <inheritdoc />
    public TenantContext Current => _current ?? throw new InvalidOperationException(
        "TenantContextProvider.Set(...) must be called (by ApiKeyAuthenticationMiddleware) before Current is read.");

    /// <summary>Sets the current request's resolved TenantContext. Called exactly once, by ApiKeyAuthenticationMiddleware.</summary>
    public void Set(TenantContext tenantContext) => _current = tenantContext;
}
