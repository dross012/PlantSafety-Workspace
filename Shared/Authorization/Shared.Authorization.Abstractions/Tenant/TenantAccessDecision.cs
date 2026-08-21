// ============================================================================
//  Project:        Shared.Authorization
//  Module:         Abstractions.Tenant
//  File:           TenantAccessDecision.cs
//  Created:        2026-08-10
//  Description:    Phase 11 deliverable. Mirrors Shared.Authorization.
//                  Abstractions.Checks.AuthorizationDecision exactly (Phase
//                  9's own pattern) -- the deterministic outcome of a
//                  TenantIsolationCheck.Evaluate call.
// ============================================================================
namespace Shared.Authorization.Abstractions.Tenant;

/// <summary>The deterministic outcome of a tenant-isolation check: whether access is allowed, and why.</summary>
public sealed record TenantAccessDecision(bool IsAuthorized, string Reason)
{
    /// <summary>Creates an allowed decision with the given reason.</summary>
    public static TenantAccessDecision Allow(string reason) => new(true, reason);

    /// <summary>Creates a denied decision with the given reason.</summary>
    public static TenantAccessDecision Deny(string reason) => new(false, reason);
}
