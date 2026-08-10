// ============================================================================
//  Project:        Shared.Authorization.Abstractions
//  Module:         Permissions
//  File:           EnterprisePermissionResolver.cs
//  Created:        2026-08-10
//  Description:    Phase 9 deliverable. Deterministic, pure, stateless
//                  tier-based permission resolution: each EnterpriseRole has
//                  a fixed tier, each ModulePermissionVerb has a fixed tier,
//                  and a role is granted every verb whose tier it meets or
//                  exceeds -- PlatformAdmin (tier 4) is granted Read/Write/
//                  Admin on every module; Viewer (tier 1) is granted only
//                  Read. An EnterpriseRole value outside the known tier map
//                  (e.g. an out-of-range cast) resolves to the empty set --
//                  zero-trust default-deny, never a fallback grant.
// ============================================================================
using Shared.Authorization.Abstractions.Roles;

namespace Shared.Authorization.Abstractions.Permissions;

/// <summary>Deterministic, pure, stateless tier-based permission resolution.</summary>
public sealed class EnterprisePermissionResolver : IPermissionResolver
{
    private static readonly IReadOnlyDictionary<EnterpriseRole, int> RoleTiers = new Dictionary<EnterpriseRole, int>
    {
        [EnterpriseRole.Viewer] = 1,
        [EnterpriseRole.Operator] = 2,
        [EnterpriseRole.FacilityManager] = 3,
        [EnterpriseRole.PlatformAdmin] = 4,
    };

    private static readonly IReadOnlyDictionary<ModulePermissionVerb, int> VerbTiers = new Dictionary<ModulePermissionVerb, int>
    {
        [ModulePermissionVerb.Read] = 1,
        [ModulePermissionVerb.Write] = 2,
        [ModulePermissionVerb.Admin] = 3,
    };

    /// <inheritdoc />
    public IReadOnlySet<string> Resolve(EnterpriseRole role, string moduleName)
    {
        ArgumentNullException.ThrowIfNull(moduleName);

        if (!RoleTiers.TryGetValue(role, out int roleTier))
        {
            // Zero-trust default-deny: an unrecognized role grants nothing.
            return new HashSet<string>();
        }

        var granted = new HashSet<string>();
        foreach ((ModulePermissionVerb verb, int verbTier) in VerbTiers)
        {
            if (roleTier >= verbTier)
            {
                granted.Add($"{moduleName}.{verb}");
            }
        }

        return granted;
    }

    /// <inheritdoc />
    public IReadOnlySet<string> ResolveAcrossModules(EnterpriseRole role, IEnumerable<string> moduleNames)
    {
        ArgumentNullException.ThrowIfNull(moduleNames);

        var granted = new HashSet<string>();
        foreach (string moduleName in moduleNames)
        {
            granted.UnionWith(Resolve(role, moduleName));
        }

        return granted;
    }
}
