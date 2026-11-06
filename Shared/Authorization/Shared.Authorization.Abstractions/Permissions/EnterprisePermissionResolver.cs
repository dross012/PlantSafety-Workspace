// ============================================================================
//  Project:        Shared.Authorization
//  Module:         Abstractions.Permissions
//  Layer:          Abstractions
//  File:           EnterprisePermissionResolver.cs
//  Created:        2026-08-02
//  Author:         Enterprise Integration & Ecosystem Expansion Agent
//  Description:    Deterministic, in-memory default implementation of
//                  IPermissionResolver. No I/O, no per-tenant overrides — pure
//                  function of (role, module) to module-qualified permission
//                  strings, derived from the enterprise role-inheritance model.
// ============================================================================

using Shared.Authorization.Abstractions.Roles;

namespace Shared.Authorization.Abstractions.Permissions;

/// <summary>
/// Default <see cref="IPermissionResolver"/> implementation. Resolves module-qualified
/// permission strings of the form <c>"{ModuleName}.{Verb}"</c> for every
/// <see cref="ModulePermissionVerb"/> granted to a role's enterprise tier and every
/// tier it inherits (see <see cref="RoleInheritanceResolver"/>).
/// </summary>
public sealed class EnterprisePermissionResolver : IPermissionResolver
{
    /// <inheritdoc />
    public IReadOnlySet<string> Resolve(EnterpriseRole role, string moduleName)
    {
        if (string.IsNullOrWhiteSpace(moduleName))
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var grantedVerbs = VerbsGrantedTo(role);

        return grantedVerbs
            .Select(verb => $"{moduleName}.{verb}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public IReadOnlySet<string> ResolveAcrossModules(EnterpriseRole role, IEnumerable<string> moduleNames)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var moduleName in moduleNames)
        {
            foreach (var permission in Resolve(role, moduleName))
            {
                result.Add(permission);
            }
        }

        return result;
    }

    /// <summary>
    /// Resolves every <see cref="ModulePermissionVerb"/> granted to <paramref name="role"/>,
    /// using the same rank ordering as <see cref="RoleInheritanceResolver"/>: a role is
    /// granted a verb when the verb's own required tier rank is at or below the role's rank.
    /// </summary>
    private static IEnumerable<ModulePermissionVerb> VerbsGrantedTo(EnterpriseRole role)
    {
        foreach (ModulePermissionVerb verb in Enum.GetValues<ModulePermissionVerb>())
        {
            if ((int)verb <= role.Rank)
            {
                yield return verb;
            }
        }
    }
}
