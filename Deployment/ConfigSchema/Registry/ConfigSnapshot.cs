// ============================================================================
//  Project:        Deployment
//  Module:         ConfigSchema.Registry
//  File:           ConfigSnapshot.cs
//  Created:        2026-08-10
//  Description:    Phase 18 deliverable. The immutable, versioned snapshot
//                  ConfigRegistry stores. ContentHash is computed by
//                  ConfigSnapshotFactory.Create via explicit, hand-built
//                  canonical string concatenation (ids sorted ordinal, one
//                  field at a time) -- never via reflection or default
//                  JSON-serialize-then-hash, so the same logical content
//                  always produces the same hash regardless of collection
//                  input order.
// ============================================================================
using System.Security.Cryptography;
using System.Text;

using Deployment.ConfigSchema.Contracts;

namespace Deployment.ConfigSchema.Registry;

/// <summary>An immutable, versioned config snapshot.</summary>
public sealed record ConfigSnapshot(
    string SnapshotId,
    string TenantId,
    int SnapshotVersion,
    DeploymentConfig DeploymentConfig,
    IReadOnlyList<TenantConfig> TenantConfigs,
    IReadOnlyList<RoleConfig> RoleConfigs,
    IReadOnlyList<ModuleConfig> ModuleConfigs,
    DateTime CreatedAtUtc,
    string ContentHash);

/// <summary>Deterministically constructs ConfigSnapshot instances, including their explicit, hand-built ContentHash.</summary>
public static class ConfigSnapshotFactory
{
    /// <summary>
    /// Creates a ConfigSnapshot for <paramref name="tenantId"/>/<paramref name="snapshotVersion"/>,
    /// with SnapshotId deterministically formatted as "{TenantId}:{SnapshotVersion}" and ContentHash
    /// computed via explicit, sorted, field-by-field canonicalization -- never reflection.
    /// </summary>
    public static ConfigSnapshot Create(
        string tenantId,
        int snapshotVersion,
        DeploymentConfig deploymentConfig,
        IReadOnlyList<TenantConfig> tenantConfigs,
        IReadOnlyList<RoleConfig> roleConfigs,
        IReadOnlyList<ModuleConfig> moduleConfigs,
        DateTime createdAtUtc)
    {
        ArgumentNullException.ThrowIfNull(tenantId);
        ArgumentNullException.ThrowIfNull(deploymentConfig);
        ArgumentNullException.ThrowIfNull(tenantConfigs);
        ArgumentNullException.ThrowIfNull(roleConfigs);
        ArgumentNullException.ThrowIfNull(moduleConfigs);

        string snapshotId = $"{tenantId}:{snapshotVersion}";
        string contentHash = ComputeContentHash(deploymentConfig, tenantConfigs, roleConfigs, moduleConfigs);

        return new ConfigSnapshot(snapshotId, tenantId, snapshotVersion, deploymentConfig, tenantConfigs, roleConfigs, moduleConfigs, createdAtUtc, contentHash);
    }

    private static string ComputeContentHash(
        DeploymentConfig deploymentConfig,
        IReadOnlyList<TenantConfig> tenantConfigs,
        IReadOnlyList<RoleConfig> roleConfigs,
        IReadOnlyList<ModuleConfig> moduleConfigs)
    {
        var builder = new StringBuilder();

        builder.Append("Deployment:").Append(deploymentConfig.DeploymentId)
            .Append('|').Append(deploymentConfig.EnvironmentName)
            .Append('|').Append(deploymentConfig.Strategy)
            .Append('|').Append(string.Join(",", deploymentConfig.ModuleIds.OrderBy(id => id, StringComparer.Ordinal)))
            .Append('|').Append(string.Join(",", deploymentConfig.CanaryStagePercentages))
            .Append('|').Append(deploymentConfig.HealthCheckTimeoutSeconds)
            .Append('|').Append(deploymentConfig.MaxRetries)
            .Append(';');

        foreach (TenantConfig tenant in tenantConfigs.OrderBy(t => t.TenantId, StringComparer.Ordinal))
        {
            builder.Append("Tenant:").Append(tenant.TenantId)
                .Append('|').Append(tenant.Topology)
                .Append('|').Append(tenant.Status)
                .Append('|').Append(tenant.ResourceQuota.MaxMachineGroups)
                .Append('|').Append(tenant.ResourceQuota.MaxConcurrentDeployments)
                .Append('|').Append(tenant.ResourceQuota.MaxStorageBytes)
                .Append('|').Append(string.Join(",", tenant.AssignedModuleIds.OrderBy(id => id, StringComparer.Ordinal)))
                .Append('|').Append(string.Join(",", tenant.AssignedRoleIds.OrderBy(id => id, StringComparer.Ordinal)))
                .Append(';');
        }

        foreach (RoleConfig role in roleConfigs.OrderBy(r => r.RoleId, StringComparer.Ordinal))
        {
            builder.Append("Role:").Append(role.RoleId)
                .Append('|').Append(string.Join(",", role.InheritsFromRoleIds.OrderBy(id => id, StringComparer.Ordinal)))
                .Append('|').Append(string.Join(",", role.GrantedPermissions.Select(p => p.ToString()).OrderBy(p => p, StringComparer.Ordinal)))
                .Append('|').Append(role.RequiresMfa)
                .Append(';');
        }

        foreach (ModuleConfig module in moduleConfigs.OrderBy(m => m.ModuleId, StringComparer.Ordinal))
        {
            builder.Append("Module:").Append(module.ModuleId)
                .Append('|').Append(module.State)
                .Append('|').Append(string.Join(",", module.FeatureFlags.Select(f => $"{f.FlagId}={f.IsEnabled}").OrderBy(f => f, StringComparer.Ordinal)))
                .Append('|').Append(string.Join(",", module.DependsOnModuleIds.OrderBy(id => id, StringComparer.Ordinal)))
                .Append(';');
        }

        byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(builder.ToString()));
        return Convert.ToHexString(hashBytes);
    }
}
