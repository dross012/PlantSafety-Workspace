// ============================================================================
//  Project:        Deployment
//  Module:         Boundaries
//  File:           RolePermissionSchema.cs
//  Created:        2026-08-10
//  Description:    Phase 18 deliverable. The three canonical, deterministic
//                  built-in roles (Viewer/Operator/Admin, tiered exactly the
//                  way Shared.Authorization.Abstractions.Permissions.
//                  EnterprisePermissionResolver already tiers EnterpriseRole
//                  for every other module) plus explicit, non-reflective
//                  role-inheritance resolution: ResolveEffectivePermissions
//                  walks RoleConfig.InheritsFromRoleIds via an explicit
//                  breadth-first traversal over a caller-supplied role
//                  catalog dictionary -- never via reflection, never via a
//                  generic object graph walker.
// ============================================================================
using Deployment.ConfigSchema.Contracts;

namespace Deployment.Boundaries;

/// <summary>The canonical, deterministic built-in Deployment roles, and explicit role-inheritance resolution.</summary>
public static class RolePermissionSchema
{
    /// <summary>The canonical RoleId for the read-only viewer role.</summary>
    public const string ViewerRoleId = "deployment-viewer";

    /// <summary>The canonical RoleId for the operator role (executes deployments).</summary>
    public const string OperatorRoleId = "deployment-operator";

    /// <summary>The canonical RoleId for the admin role (approves, manages tenants/config/secrets).</summary>
    public const string AdminRoleId = "deployment-admin";

    /// <summary>Builds the canonical Viewer RoleConfig: ViewDeployments only, no inheritance, no MFA.</summary>
    public static RoleConfig BuildViewerRole(DeploymentContractMetadata metadata) => new(
        metadata, ViewerRoleId, "Deployment Viewer", Array.Empty<string>(), new[] { DeploymentPermission.ViewDeployments }, RequiresMfa: false);

    /// <summary>Builds the canonical Operator RoleConfig: inherits Viewer, adds ExecuteDeployments, no MFA.</summary>
    public static RoleConfig BuildOperatorRole(DeploymentContractMetadata metadata) => new(
        metadata, OperatorRoleId, "Deployment Operator", new[] { ViewerRoleId }, new[] { DeploymentPermission.ExecuteDeployments }, RequiresMfa: false);

    /// <summary>Builds the canonical Admin RoleConfig: inherits Operator, adds every sensitive permission, requires MFA.</summary>
    public static RoleConfig BuildAdminRole(DeploymentContractMetadata metadata) => new(
        metadata,
        AdminRoleId,
        "Deployment Admin",
        new[] { OperatorRoleId },
        new[] { DeploymentPermission.ApproveDeployments, DeploymentPermission.ManageTenants, DeploymentPermission.ManageConfig, DeploymentPermission.ManageSecrets },
        RequiresMfa: true);

    /// <summary>
    /// Deterministically resolves every permission <paramref name="roleId"/> is granted, directly or
    /// via inheritance, by an explicit breadth-first walk over <paramref name="roleCatalog"/>.
    /// Cycle-safe: a role already visited is never re-walked. Throws KeyNotFoundException if
    /// <paramref name="roleId"/> or any inherited role id is not present in the catalog.
    /// </summary>
    public static IReadOnlySet<DeploymentPermission> ResolveEffectivePermissions(string roleId, IReadOnlyDictionary<string, RoleConfig> roleCatalog)
    {
        ArgumentNullException.ThrowIfNull(roleId);
        ArgumentNullException.ThrowIfNull(roleCatalog);

        var effectivePermissions = new HashSet<DeploymentPermission>();
        var visitedRoleIds = new HashSet<string>(StringComparer.Ordinal);
        var queue = new Queue<string>();
        queue.Enqueue(roleId);

        while (queue.Count > 0)
        {
            string currentRoleId = queue.Dequeue();
            if (!visitedRoleIds.Add(currentRoleId))
            {
                continue;
            }

            if (!roleCatalog.TryGetValue(currentRoleId, out RoleConfig? role))
            {
                throw new KeyNotFoundException($"RoleId \"{currentRoleId}\" was not found in the supplied role catalog.");
            }

            effectivePermissions.UnionWith(role.GrantedPermissions);

            foreach (string inheritedRoleId in role.InheritsFromRoleIds)
            {
                if (!visitedRoleIds.Contains(inheritedRoleId))
                {
                    queue.Enqueue(inheritedRoleId);
                }
            }
        }

        return effectivePermissions;
    }

    /// <summary>Deterministically resolves whether MFA is required for <paramref name="roleId"/>: true if the role itself or any inherited role requires it.</summary>
    public static bool ResolveRequiresMfa(string roleId, IReadOnlyDictionary<string, RoleConfig> roleCatalog)
    {
        ArgumentNullException.ThrowIfNull(roleId);
        ArgumentNullException.ThrowIfNull(roleCatalog);

        var visitedRoleIds = new HashSet<string>(StringComparer.Ordinal);
        var queue = new Queue<string>();
        queue.Enqueue(roleId);

        while (queue.Count > 0)
        {
            string currentRoleId = queue.Dequeue();
            if (!visitedRoleIds.Add(currentRoleId))
            {
                continue;
            }

            if (!roleCatalog.TryGetValue(currentRoleId, out RoleConfig? role))
            {
                throw new KeyNotFoundException($"RoleId \"{currentRoleId}\" was not found in the supplied role catalog.");
            }

            if (role.RequiresMfa)
            {
                return true;
            }

            foreach (string inheritedRoleId in role.InheritsFromRoleIds)
            {
                if (!visitedRoleIds.Contains(inheritedRoleId))
                {
                    queue.Enqueue(inheritedRoleId);
                }
            }
        }

        return false;
    }
}
