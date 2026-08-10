// ============================================================================
//  Project:        Shared.Authorization.Infrastructure
//  Module:         Options
//  File:           TenantAuthenticationOptions.cs
//  Created:        2026-08-10
//  Description:    Phase 11 deliverable. Bound once from configuration
//                  ("TenantAuthentication:RegisteredKeyTenants") in
//                  Program.cs, independently of Phase 10's key -> role map --
//                  a key may be registered for one, both, or neither.
//                  Defaults to empty, so with no configuration present every
//                  request resolves to TenantContext.Unscoped, exactly
//                  reproducing this program's pre-Phase-11 behavior.
// ============================================================================
namespace Shared.Authorization.Infrastructure.Options;

/// <summary>Configuration-bound options for API-key -> TenantId resolution.</summary>
public sealed class TenantAuthenticationOptions
{
    /// <summary>The configuration section name this options type binds from.</summary>
    public const string SectionName = "TenantAuthentication";

    /// <summary>Raw API-key string -> TenantId. Defaults to empty.</summary>
    public Dictionary<string, string> RegisteredKeyTenants { get; set; } = new();
}
