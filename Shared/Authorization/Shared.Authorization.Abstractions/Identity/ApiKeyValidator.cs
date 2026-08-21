// ============================================================================
//  Project:        Shared.Authorization
//  Module:         Abstractions.Identity
//  File:           ApiKeyValidator.cs
//  Created:        2026-08-10
//  Description:    Phase 10 (Authentication Layer Modernization) deliverable.
//                  Pure, deterministic API-key -> EnterpriseRole resolution.
//                  No I/O, no external identity provider, no OAuth/OIDC/JWT
//                  -- a plain dictionary lookup against a caller-supplied,
//                  statically-configured registration map. Unknown or
//                  missing keys resolve to null, never an exception and
//                  never a guessed role -- the caller (CallerContextFactory)
//                  decides what null means (today: fall back to
//                  CallerContext.SystemDefault(), preserving full backward
//                  compatibility, per this phase's explicit "no endpoint
//                  breaks" requirement).
// ============================================================================
using Shared.Authorization.Abstractions.Roles;

namespace Shared.Authorization.Abstractions.Identity;

/// <summary>Deterministically resolves a raw API key string to an EnterpriseRole via a static registration map.</summary>
public static class ApiKeyValidator
{
    /// <summary>
    /// Resolves <paramref name="apiKey"/> to the EnterpriseRole registered for it in
    /// <paramref name="registeredKeys"/>. Returns null when <paramref name="apiKey"/> is null,
    /// empty, whitespace, or not present in <paramref name="registeredKeys"/> -- never throws,
    /// never guesses.
    /// </summary>
    public static EnterpriseRole? Resolve(string? apiKey, IReadOnlyDictionary<string, EnterpriseRole> registeredKeys)
    {
        ArgumentNullException.ThrowIfNull(registeredKeys);

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return null;
        }

        return registeredKeys.TryGetValue(apiKey, out EnterpriseRole? role) ? role : null;
    }
}
