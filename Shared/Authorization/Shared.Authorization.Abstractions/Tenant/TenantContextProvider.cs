// ============================================================================
//  Project:        Shared.Authorization.Abstractions
//  Module:         Tenant
//  File:           TenantContextProvider.cs
//  Created:        2026-08-10
//  Description:    Phase 11 deliverable. Request-scoped write-once holder,
//                  mirroring CallerContextProvider exactly.
// ============================================================================
namespace Shared.Authorization.Abstractions.Tenant;

/// <summary>Request-scoped write-once holder for the resolved TenantContext.</summary>
public sealed class TenantContextProvider : ITenantContextProvider
{
    private TenantContext? _current;

    /// <inheritdoc />
    public TenantContext Current => _current
        ?? throw new InvalidOperationException("TenantContext has not been set for this request. ApiKeyAuthenticationMiddleware must run before it is read.");

    /// <summary>Sets the resolved TenantContext for this request. Called once, by ApiKeyAuthenticationMiddleware.</summary>
    public void Set(TenantContext tenantContext)
    {
        ArgumentNullException.ThrowIfNull(tenantContext);
        _current = tenantContext;
    }
}
