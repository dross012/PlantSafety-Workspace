// ============================================================================
//  Project:        Shared.Authorization
//  Module:         Abstractions.Tenant
//  File:           ITenantContextProvider.cs
//  Created:        2026-08-10
//  Description:    Phase 11 deliverable. Mirrors Shared.Authorization.
//                  Abstractions.Caller.ICallerContextProvider (Phase 10)
//                  exactly, which itself mirrors Shared.Telemetry.
//                  Abstractions.Correlation.ICorrelationContext's Set-once-
//                  then-read pattern: the authentication middleware calls
//                  Set(...) once, after which downstream consumers read
//                  Current instead of constructing a TenantContext inline.
// ============================================================================
namespace Shared.Authorization.Abstractions.Tenant;

/// <summary>The request-scoped carrier for the current request's resolved TenantContext.</summary>
public interface ITenantContextProvider
{
    /// <summary>The current request's resolved TenantContext. Throws if read before Set(...) is called.</summary>
    TenantContext Current { get; }
}
