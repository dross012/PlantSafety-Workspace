// ============================================================================
//  Project:        Deployment
//  Module:         Boundaries
//  File:           RoleValidator.cs
//  Created:        2026-08-10
//  Description:    Phase 18 deliverable. Pure, deterministic validation of a
//                  role catalog: dangling inheritance references, inheritance
//                  cycles (explicit white/gray/black DFS, the same algorithm
//                  Shared.SafetyGraph.Abstractions.Traversal.
//                  SafetyGraphTraversalEngine.DetectCycle uses for graph
//                  edges, Phase 16), and the deterministic policy that any
//                  role granting a sensitive permission (Approve/ManageTenants/
//                  ManageConfig/ManageSecrets) must require MFA either
//                  directly or via inheritance.
// ============================================================================
using Deployment.ConfigSchema.Contracts;

namespace Deployment.Boundaries;

/// <summary>Pure, deterministic validation of a Deployment role catalog.</summary>
public static class RoleValidator
{
    private static readonly IReadOnlySet<DeploymentPermission> SensitivePermissions = new HashSet<DeploymentPermission>
    {
        DeploymentPermission.ApproveDeployments,
        DeploymentPermission.ManageTenants,
        DeploymentPermission.ManageConfig,
        DeploymentPermission.ManageSecrets,
    };

    /// <summary>
    /// Validates <paramref name="roleCatalog"/>: every InheritsFromRoleIds reference must resolve, the
    /// inheritance graph must be acyclic, and any role whose effective permissions include a sensitive
    /// permission must effectively require MFA. Returns every violation found (empty if valid).
    /// </summary>
    public static IReadOnlyList<string> ValidateRoleCatalog(IReadOnlyDictionary<string, RoleConfig> roleCatalog)
    {
        ArgumentNullException.ThrowIfNull(roleCatalog);

        var violations = new List<string>();

        foreach (RoleConfig role in roleCatalog.Values.OrderBy(r => r.RoleId, StringComparer.Ordinal))
        {
            foreach (string inheritedRoleId in role.InheritsFromRoleIds)
            {
                if (!roleCatalog.ContainsKey(inheritedRoleId))
                {
                    violations.Add($"Role \"{role.RoleId}\" inherits from unknown role \"{inheritedRoleId}\".");
                }
            }
        }

        violations.AddRange(DetectInheritanceCycles(roleCatalog));

        foreach (RoleConfig role in roleCatalog.Values.OrderBy(r => r.RoleId, StringComparer.Ordinal))
        {
            if (!role.GrantedPermissions.Any(SensitivePermissions.Contains))
            {
                continue;
            }

            bool effectivelyRequiresMfa;
            try
            {
                effectivelyRequiresMfa = RolePermissionSchema.ResolveRequiresMfa(role.RoleId, roleCatalog);
            }
            catch (KeyNotFoundException)
            {
                // Already reported above as a dangling reference; skip the MFA check for this role.
                continue;
            }

            if (!effectivelyRequiresMfa)
            {
                violations.Add($"Role \"{role.RoleId}\" grants a sensitive permission but does not require MFA (directly or via inheritance).");
            }
        }

        return violations;
    }

    private static IReadOnlyList<string> DetectInheritanceCycles(IReadOnlyDictionary<string, RoleConfig> roleCatalog)
    {
        var violations = new List<string>();
        var visitedState = new Dictionary<string, int>(StringComparer.Ordinal); // 0=white, 1=gray, 2=black

        foreach (string roleId in roleCatalog.Keys.OrderBy(id => id, StringComparer.Ordinal))
        {
            if (visitedState.GetValueOrDefault(roleId, 0) == 0)
            {
                string? cycleRoleId = DetectCycleFrom(roleId, roleCatalog, visitedState);
                if (cycleRoleId is not null)
                {
                    violations.Add($"Role inheritance cycle detected involving \"{cycleRoleId}\".");
                }
            }
        }

        return violations;
    }

    private static string? DetectCycleFrom(string roleId, IReadOnlyDictionary<string, RoleConfig> roleCatalog, Dictionary<string, int> visitedState)
    {
        visitedState[roleId] = 1;

        if (roleCatalog.TryGetValue(roleId, out RoleConfig? role))
        {
            foreach (string inheritedRoleId in role.InheritsFromRoleIds)
            {
                int state = visitedState.GetValueOrDefault(inheritedRoleId, 0);
                if (state == 1)
                {
                    return inheritedRoleId;
                }

                if (state == 0 && roleCatalog.ContainsKey(inheritedRoleId))
                {
                    string? found = DetectCycleFrom(inheritedRoleId, roleCatalog, visitedState);
                    if (found is not null)
                    {
                        return found;
                    }
                }
            }
        }

        visitedState[roleId] = 2;
        return null;
    }
}
