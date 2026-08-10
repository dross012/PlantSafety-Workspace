// ============================================================================
//  Project:        Shared.Authorization.Abstractions
//  Module:         Permissions
//  File:           IPermissionResolver.cs
//  Created:        2026-08-10
//  Description:    Phase 9 deliverable. The seam AuthorizationCheck consults
//                  to resolve a role's granted permissions. Registered as
//                  IPermissionResolver -> EnterprisePermissionResolver in
//                  Program.cs (singleton, stateless, pure).
// ============================================================================
using Shared.Authorization.Abstractions.Roles;

namespace Shared.Authorization.Abstractions.Permissions;

/// <summary>Resolves the set of "{module}.{verb}" permission strings a role is granted.</summary>
public interface IPermissionResolver
{
    /// <summary>Resolves the "{<paramref name="moduleName"/>}.{verb}" permissions <paramref name="role"/> is granted.</summary>
    IReadOnlySet<string> Resolve(EnterpriseRole role, string moduleName);

    /// <summary>Resolves the permissions <paramref name="role"/> is granted across every module in <paramref name="moduleNames"/>.</summary>
    IReadOnlySet<string> ResolveAcrossModules(EnterpriseRole role, IEnumerable<string> moduleNames);
}
