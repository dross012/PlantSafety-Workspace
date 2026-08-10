// ============================================================================
//  Project:        Deployment
//  Module:         Governance
//  File:           DeploymentGovernanceRules.cs
//  Created:        2026-08-10
//  Description:    Phase 18 deliverable. Pure, deterministic deployment
//                  policy rules -- distinct from ConfigSchema.Registry.
//                  ConfigValidator, which checks structural validity
//                  (well-formedness). These rules check POLICY: is this
//                  tenant allowed to deploy right now, does it have quota
//                  headroom, does a production deployment have an assigned
//                  role capable of approving it. Every rule is a pure
//                  function -- input in, violation list out, zero side
//                  effects, mirroring every prior phase's *GovernanceRules.
// ============================================================================
using Deployment.Boundaries;
using Deployment.ConfigSchema.Contracts;

namespace Deployment.Governance;

/// <summary>Pure, deterministic deployment policy rules.</summary>
public static class DeploymentGovernanceRules
{
    /// <summary>The EnvironmentName value (case-insensitive) that requires an approval-capable role to be assigned before deploying.</summary>
    public const string ProductionEnvironmentName = "production";

    /// <summary>Validates that <paramref name="tenant"/> is Active. Returns every violation found (empty if valid).</summary>
    public static IReadOnlyList<string> ValidateTenantEligibleForDeployment(TenantConfig tenant)
    {
        ArgumentNullException.ThrowIfNull(tenant);

        var violations = new List<string>();

        if (tenant.Status != TenantStatus.Active)
        {
            violations.Add($"Tenant \"{tenant.TenantId}\" is not Active (current status: {tenant.Status}) -- deployments are blocked for non-Active tenants.");
        }

        return violations;
    }

    /// <summary>Validates that <paramref name="requestedConcurrentDeployments"/> does not exceed <paramref name="tenant"/>'s quota. Returns every violation found (empty if valid).</summary>
    public static IReadOnlyList<string> ValidateResourceQuota(TenantConfig tenant, int requestedConcurrentDeployments)
    {
        ArgumentNullException.ThrowIfNull(tenant);

        var violations = new List<string>();

        if (requestedConcurrentDeployments > tenant.ResourceQuota.MaxConcurrentDeployments)
        {
            violations.Add(
                $"Tenant \"{tenant.TenantId}\" requested {requestedConcurrentDeployments} concurrent deployment(s), exceeding its quota of {tenant.ResourceQuota.MaxConcurrentDeployments}.");
        }

        return violations;
    }

    /// <summary>
    /// Validates that, when <paramref name="config"/>.EnvironmentName is "production" (case-insensitive),
    /// at least one of <paramref name="assignedRoleIds"/> effectively grants
    /// DeploymentPermission.ApproveDeployments (directly or via inheritance, resolved through
    /// <paramref name="roleCatalog"/>). Returns every violation found (empty if valid or non-production).
    /// </summary>
    public static IReadOnlyList<string> ValidateProductionRequiresApprovalRole(
        DeploymentConfig config,
        IReadOnlyDictionary<string, RoleConfig> roleCatalog,
        IReadOnlyList<string> assignedRoleIds)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(roleCatalog);
        ArgumentNullException.ThrowIfNull(assignedRoleIds);

        var violations = new List<string>();

        if (!string.Equals(config.EnvironmentName, ProductionEnvironmentName, StringComparison.OrdinalIgnoreCase))
        {
            return violations;
        }

        bool hasApprovalCapableRole = false;
        foreach (string roleId in assignedRoleIds)
        {
            if (!roleCatalog.ContainsKey(roleId))
            {
                continue;
            }

            IReadOnlySet<DeploymentPermission> effectivePermissions = RolePermissionSchema.ResolveEffectivePermissions(roleId, roleCatalog);
            if (effectivePermissions.Contains(DeploymentPermission.ApproveDeployments))
            {
                hasApprovalCapableRole = true;
                break;
            }
        }

        if (!hasApprovalCapableRole)
        {
            violations.Add($"DeploymentConfig \"{config.DeploymentId}\" targets \"{ProductionEnvironmentName}\" but no assigned role grants ApproveDeployments.");
        }

        return violations;
    }
}
