// ============================================================================
//  Project:        Shared.Authorization
//  Module:         Abstractions.Tenant
//  File:           TenantAuthenticationOptions.cs
//  Created:        2026-08-10
//  Description:    Phase 11 deliverable. Statically-configured API key ->
//                  TenantId registration map, bound from configuration (e.g.
//                  "TenantAuthentication:RegisteredKeyTenants" -- additive to,
//                  and independent of, Phase 10's ApiKeyAuthenticationOptions
//                  role map). Defaults to empty: no real caller sends a
//                  tenant-bound API key today, so an empty map reproduces the
//                  exact pre-Phase-11 behavior (every request resolves to
//                  TenantContext.Unscoped) until an operator registers a
//                  binding.
// ============================================================================
namespace Shared.Authorization.Abstractions.Tenant;

/// <summary>The statically-configured API key -> TenantId registration map this host resolves tenant scope from.</summary>
public sealed class TenantAuthenticationOptions
{
    /// <summary>The configuration section name this type binds from.</summary>
    public const string SectionName = "TenantAuthentication";

    /// <summary>Raw API key -> TenantId pairs, as bound from configuration.</summary>
    public Dictionary<string, string> RegisteredKeyTenants { get; init; } = new();
}
