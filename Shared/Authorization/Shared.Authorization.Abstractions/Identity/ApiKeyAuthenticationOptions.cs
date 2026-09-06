// ============================================================================
//  Project:        Shared.Authorization
//  Module:         Abstractions.Identity
//  File:           ApiKeyAuthenticationOptions.cs
//  Created:        2026-08-10
//  Description:    Phase 10 deliverable. Statically-configured API key ->
//                  EnterpriseRole registration map, bound from configuration
//                  (e.g. "ApiKeyAuthentication:RegisteredKeys" in
//                  appsettings/environment variables/secrets -- never
//                  hardcoded). Defaults to empty, which is deliberate: no
//                  real caller sends an API key today, so an empty map
//                  reproduces today's exact behavior (every request falls
//                  back to CallerContext.SystemDefault) until an operator
//                  actually registers a key.
// ============================================================================
using Shared.Authorization.Abstractions.Roles;

namespace Shared.Authorization.Abstractions.Identity;

/// <summary>The statically-configured API key registration map this host authenticates requests against.</summary>
public sealed class ApiKeyAuthenticationOptions
{
    /// <summary>The configuration section name this type binds from.</summary>
    public const string SectionName = "ApiKeyAuthentication";

    /// <summary>Raw key -> EnterpriseRole.Name (e.g. "Safety.Editor") pairs, as bound from configuration.</summary>
    public Dictionary<string, string> RegisteredKeys { get; init; } = new();

    /// <summary>
    /// Resolves <see cref="RegisteredKeys"/> into a key -> EnterpriseRole map, dropping (never
    /// throwing on) any entry whose role name does not match a canonical EnterpriseRole -- an
    /// operator typo in configuration must not crash the host at startup.
    /// </summary>
    public IReadOnlyDictionary<string, EnterpriseRole> ToRoleMap()
    {
        var map = new Dictionary<string, EnterpriseRole>();
        foreach (KeyValuePair<string, string> entry in RegisteredKeys)
        {
            EnterpriseRole? role = EnterpriseRole.FromClaimValue(entry.Value);
            if (role is not null)
            {
                map[entry.Key] = role;
            }
        }

        return map;
    }
}
