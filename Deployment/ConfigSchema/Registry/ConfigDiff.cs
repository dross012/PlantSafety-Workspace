// ============================================================================
//  Project:        Deployment
//  Module:         ConfigSchema.Registry
//  File:           ConfigDiff.cs
//  Created:        2026-08-10
//  Description:    Phase 18 deliverable. Deterministic, pure, explicit
//                  field-by-field comparison between two ConfigSnapshots --
//                  no reflection, no generic object-graph differ. Every
//                  compared collection is sorted by its own stable id
//                  before comparison, so a snapshot re-serialized with
//                  reordered collections never reports a spurious change.
// ============================================================================
using Deployment.ConfigSchema.Contracts;

namespace Deployment.ConfigSchema.Registry;

/// <summary>A single, deterministic detected change between two snapshots.</summary>
public sealed record ConfigChange(string FieldPath, string? OldValue, string? NewValue);

/// <summary>The deterministic result of comparing two ConfigSnapshots.</summary>
public sealed record ConfigDiffResult(
    string FromSnapshotId,
    string ToSnapshotId,
    IReadOnlyList<ConfigChange> Changes,
    bool HasChanges);

/// <summary>Deterministic, pure, explicit field-by-field snapshot comparison.</summary>
public static class ConfigDiff
{
    /// <summary>Compares <paramref name="from"/> and <paramref name="to"/> field by field. Same inputs always produce the same output.</summary>
    public static ConfigDiffResult Compare(ConfigSnapshot from, ConfigSnapshot to)
    {
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);

        var changes = new List<ConfigChange>();

        CompareDeploymentConfig(from.DeploymentConfig, to.DeploymentConfig, changes);
        CompareModuleConfigs(from.ModuleConfigs, to.ModuleConfigs, changes);
        CompareTenantConfigs(from.TenantConfigs, to.TenantConfigs, changes);
        CompareRoleConfigs(from.RoleConfigs, to.RoleConfigs, changes);

        return new ConfigDiffResult(from.SnapshotId, to.SnapshotId, changes, changes.Count > 0);
    }

    private static void CompareDeploymentConfig(DeploymentConfig from, DeploymentConfig to, List<ConfigChange> changes)
    {
        AddIfChanged(changes, "DeploymentConfig.EnvironmentName", from.EnvironmentName, to.EnvironmentName);
        AddIfChanged(changes, "DeploymentConfig.Strategy", from.Strategy.ToString(), to.Strategy.ToString());
        AddIfChanged(changes, "DeploymentConfig.ModuleIds", JoinSorted(from.ModuleIds), JoinSorted(to.ModuleIds));
        AddIfChanged(changes, "DeploymentConfig.CanaryStagePercentages", string.Join(",", from.CanaryStagePercentages), string.Join(",", to.CanaryStagePercentages));
        AddIfChanged(changes, "DeploymentConfig.HealthCheckTimeoutSeconds", from.HealthCheckTimeoutSeconds.ToString(), to.HealthCheckTimeoutSeconds.ToString());
        AddIfChanged(changes, "DeploymentConfig.MaxRetries", from.MaxRetries.ToString(), to.MaxRetries.ToString());
    }

    private static void CompareModuleConfigs(IReadOnlyList<ModuleConfig> from, IReadOnlyList<ModuleConfig> to, List<ConfigChange> changes)
    {
        Dictionary<string, ModuleConfig> fromById = from.ToDictionary(m => m.ModuleId, StringComparer.Ordinal);
        Dictionary<string, ModuleConfig> toById = to.ToDictionary(m => m.ModuleId, StringComparer.Ordinal);

        foreach (string moduleId in fromById.Keys.Union(toById.Keys, StringComparer.Ordinal).OrderBy(id => id, StringComparer.Ordinal))
        {
            bool inFrom = fromById.TryGetValue(moduleId, out ModuleConfig? fromModule);
            bool inTo = toById.TryGetValue(moduleId, out ModuleConfig? toModule);

            if (inFrom && !inTo)
            {
                changes.Add(new ConfigChange($"ModuleConfig[{moduleId}]", "present", null));
            }
            else if (!inFrom && inTo)
            {
                changes.Add(new ConfigChange($"ModuleConfig[{moduleId}]", null, "present"));
            }
            else
            {
                AddIfChanged(changes, $"ModuleConfig[{moduleId}].State", fromModule!.State.ToString(), toModule!.State.ToString());
                AddIfChanged(changes, $"ModuleConfig[{moduleId}].DependsOnModuleIds", JoinSorted(fromModule.DependsOnModuleIds), JoinSorted(toModule.DependsOnModuleIds));
                AddIfChanged(
                    changes,
                    $"ModuleConfig[{moduleId}].FeatureFlags",
                    string.Join(",", fromModule.FeatureFlags.Select(f => $"{f.FlagId}={f.IsEnabled}").OrderBy(f => f, StringComparer.Ordinal)),
                    string.Join(",", toModule.FeatureFlags.Select(f => $"{f.FlagId}={f.IsEnabled}").OrderBy(f => f, StringComparer.Ordinal)));
            }
        }
    }

    private static void CompareTenantConfigs(IReadOnlyList<TenantConfig> from, IReadOnlyList<TenantConfig> to, List<ConfigChange> changes)
    {
        Dictionary<string, TenantConfig> fromById = from.ToDictionary(t => t.TenantId, StringComparer.Ordinal);
        Dictionary<string, TenantConfig> toById = to.ToDictionary(t => t.TenantId, StringComparer.Ordinal);

        foreach (string tenantId in fromById.Keys.Union(toById.Keys, StringComparer.Ordinal).OrderBy(id => id, StringComparer.Ordinal))
        {
            bool inFrom = fromById.TryGetValue(tenantId, out TenantConfig? fromTenant);
            bool inTo = toById.TryGetValue(tenantId, out TenantConfig? toTenant);

            if (inFrom && !inTo)
            {
                changes.Add(new ConfigChange($"TenantConfig[{tenantId}]", "present", null));
            }
            else if (!inFrom && inTo)
            {
                changes.Add(new ConfigChange($"TenantConfig[{tenantId}]", null, "present"));
            }
            else
            {
                AddIfChanged(changes, $"TenantConfig[{tenantId}].Status", fromTenant!.Status.ToString(), toTenant!.Status.ToString());
                AddIfChanged(changes, $"TenantConfig[{tenantId}].Topology", fromTenant.Topology.ToString(), toTenant.Topology.ToString());
                AddIfChanged(changes, $"TenantConfig[{tenantId}].AssignedModuleIds", JoinSorted(fromTenant.AssignedModuleIds), JoinSorted(toTenant.AssignedModuleIds));
                AddIfChanged(changes, $"TenantConfig[{tenantId}].AssignedRoleIds", JoinSorted(fromTenant.AssignedRoleIds), JoinSorted(toTenant.AssignedRoleIds));
            }
        }
    }

    private static void CompareRoleConfigs(IReadOnlyList<RoleConfig> from, IReadOnlyList<RoleConfig> to, List<ConfigChange> changes)
    {
        Dictionary<string, RoleConfig> fromById = from.ToDictionary(r => r.RoleId, StringComparer.Ordinal);
        Dictionary<string, RoleConfig> toById = to.ToDictionary(r => r.RoleId, StringComparer.Ordinal);

        foreach (string roleId in fromById.Keys.Union(toById.Keys, StringComparer.Ordinal).OrderBy(id => id, StringComparer.Ordinal))
        {
            bool inFrom = fromById.TryGetValue(roleId, out RoleConfig? fromRole);
            bool inTo = toById.TryGetValue(roleId, out RoleConfig? toRole);

            if (inFrom && !inTo)
            {
                changes.Add(new ConfigChange($"RoleConfig[{roleId}]", "present", null));
            }
            else if (!inFrom && inTo)
            {
                changes.Add(new ConfigChange($"RoleConfig[{roleId}]", null, "present"));
            }
            else
            {
                AddIfChanged(
                    changes,
                    $"RoleConfig[{roleId}].GrantedPermissions",
                    string.Join(",", fromRole!.GrantedPermissions.Select(p => p.ToString()).OrderBy(p => p, StringComparer.Ordinal)),
                    string.Join(",", toRole!.GrantedPermissions.Select(p => p.ToString()).OrderBy(p => p, StringComparer.Ordinal)));
                AddIfChanged(changes, $"RoleConfig[{roleId}].RequiresMfa", fromRole.RequiresMfa.ToString(), toRole.RequiresMfa.ToString());
            }
        }
    }

    private static string JoinSorted(IReadOnlyList<string> values) => string.Join(",", values.OrderBy(v => v, StringComparer.Ordinal));

    private static void AddIfChanged(List<ConfigChange> changes, string fieldPath, string? oldValue, string? newValue)
    {
        if (!string.Equals(oldValue, newValue, StringComparison.Ordinal))
        {
            changes.Add(new ConfigChange(fieldPath, oldValue, newValue));
        }
    }
}
