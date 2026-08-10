// ============================================================================
//  Project:        Shared.Authorization.Infrastructure
//  Module:         Options
//  File:           ApiKeyAuthenticationOptions.cs
//  Created:        2026-08-10
//  Description:    Phase 10 deliverable. Bound once from configuration
//                  ("ApiKeyAuthentication:RegisteredKeys") in Program.cs.
//                  RegisteredKeys maps a raw API-key string to an
//                  EnterpriseRole name (parsed case-insensitively);
//                  unparseable entries are skipped rather than throwing --
//                  a malformed configuration entry should not crash startup,
//                  it should simply not grant that key any role (the same
//                  zero-trust default-deny EnterprisePermissionResolver
//                  applies to an unrecognized role). Defaults to empty, so
//                  with no configuration present every request resolves to
//                  CallerContext.SystemDefault, exactly reproducing this
//                  program's pre-Phase-10 behavior.
// ============================================================================
using Shared.Authorization.Abstractions.Roles;

namespace Shared.Authorization.Infrastructure.Options;

/// <summary>Configuration-bound options for API-key -> EnterpriseRole resolution.</summary>
public sealed class ApiKeyAuthenticationOptions
{
    /// <summary>The configuration section name this options type binds from.</summary>
    public const string SectionName = "ApiKeyAuthentication";

    /// <summary>Raw API-key string -> EnterpriseRole name (parsed case-insensitively). Defaults to empty.</summary>
    public Dictionary<string, string> RegisteredKeys { get; set; } = new();

    /// <summary>Parses RegisteredKeys into an API-key -> EnterpriseRole map, skipping any entry whose role name does not parse.</summary>
    public IReadOnlyDictionary<string, EnterpriseRole> ToRoleMap()
    {
        var map = new Dictionary<string, EnterpriseRole>();
        foreach ((string apiKey, string roleName) in RegisteredKeys)
        {
            if (Enum.TryParse(roleName, ignoreCase: true, out EnterpriseRole role))
            {
                map[apiKey] = role;
            }
        }

        return map;
    }
}
