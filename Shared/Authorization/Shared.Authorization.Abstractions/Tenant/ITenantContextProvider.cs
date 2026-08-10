// ============================================================================
//  Project:        Shared.Authorization.Abstractions
//  Module:         Tenant
//  File:           ITenantContextProvider.cs
//  Created:        2026-08-10
//  Description:    Phase 11 deliverable. The read-side seam controllers use,
//                  mirroring ICallerContextProvider exactly.
// ============================================================================
namespace Shared.Authorization.Abstractions.Tenant;

/// <summary>The read-side seam for the current request's resolved TenantContext.</summary>
public interface ITenantContextProvider
{
    /// <summary>The current request's resolved TenantContext. Throws if read before ApiKeyAuthenticationMiddleware has set it.</summary>
    TenantContext Current { get; }
}
