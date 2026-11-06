// ============================================================================
//  Project:        Deployment
//  Module:         ConfigSchema.Registry
//  File:           ConfigValidator.cs
//  Created:        2026-08-10
//  Description:    Phase 18 deliverable. The pre-deploy validation pipeline:
//                  pure, deterministic rules over DeploymentConfig/
//                  TenantConfig/RoleConfig/ModuleConfig, mirroring every
//                  prior phase's *GovernanceRules pattern exactly. Module
//                  dependency cycle detection uses the same explicit white/
//                  gray/black DFS RoleValidator/SafetyGraphTraversalEngine
//                  already use -- never reflection.
// ============================================================================
using Deployment.Boundaries;
using Deployment.ConfigSchema.Contracts;

namespace Deployment.ConfigSchema.Registry;

/// <summary>The pure, deterministic pre-deploy validation pipeline for a full config snapshot.</summary>
public static class ConfigValidator
{
    /// <summary>Validates <paramref name="config"/>. Returns every violation found (empty if valid).</summary>
    public static IReadOnlyList<string> ValidateDeploymentConfig(DeploymentConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        var violations = new List<string>();

        if (string.IsNullOrWhiteSpace(config.DeploymentId))
        {
            violations.Add("DeploymentConfig.DeploymentId is missing or empty.");
        }

        if (string.IsNullOrWhiteSpace(config.EnvironmentName))
        {
            violations.Add("DeploymentConfig.EnvironmentName is missing or empty.");
        }

        if (config.ModuleIds.Count == 0)
        {
            violations.Add("DeploymentConfig.ModuleIds is empty -- a deployment must target at least one module.");
        }

        if (config.HealthCheckTimeoutSeconds <= 0)
        {
            violations.Add("DeploymentConfig.HealthCheckTimeoutSeconds must be positive.");
        }

        if (config.MaxRetries < 0)
        {
            violations.Add("DeploymentConfig.MaxRetries must not be negative.");
        }

        if (config.Strategy == RolloutStrategyType.Canary)
        {
            violations.AddRange(ValidateCanaryStages(config.CanaryStagePercentages));
        }
        else if (config.CanaryStagePercentages.Count > 0)
        {
            violations.Add("DeploymentConfig.CanaryStagePercentages must be empty when Strategy is BlueGreen.");
        }

        return violations;
    }

    private static IReadOnlyList<string> ValidateCanaryStages(IReadOnlyList<int> stages)
    {
        var violations = new List<string>();

        if (stages.Count == 0)
        {
            violations.Add("DeploymentConfig.CanaryStagePercentages must be non-empty when Strategy is Canary.");
            return violations;
        }

        for (int i = 0; i < stages.Count; i++)
        {
            if (stages[i] <= 0 || stages[i] > 100)
            {
                violations.Add($"DeploymentConfig.CanaryStagePercentages[{i}] ({stages[i]}) must be between 1 and 100.");
            }

            if (i > 0 && stages[i] <= stages[i - 1])
            {
                violations.Add($"DeploymentConfig.CanaryStagePercentages must be strictly ascending -- stage {i} ({stages[i]}) does not exceed stage {i - 1} ({stages[i - 1]}).");
            }
        }

        if (stages[^1] != 100)
        {
            violations.Add("DeploymentConfig.CanaryStagePercentages must end at 100.");
        }

        return violations;
    }

    /// <summary>Validates <paramref name="config"/>. Returns every violation found (empty if valid).</summary>
    public static IReadOnlyList<string> ValidateTenantConfig(TenantConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        var violations = new List<string>();

        if (string.IsNullOrWhiteSpace(config.TenantId))
        {
            violations.Add("TenantConfig.TenantId is missing or empty.");
        }

        if (string.IsNullOrWhiteSpace(config.TenantName))
        {
            violations.Add("TenantConfig.TenantName is missing or empty.");
        }

        if (config.ResourceQuota.MaxMachineGroups <= 0)
        {
            violations.Add("TenantConfig.ResourceQuota.MaxMachineGroups must be positive.");
        }

        if (config.ResourceQuota.MaxConcurrentDeployments <= 0)
        {
            violations.Add("TenantConfig.ResourceQuota.MaxConcurrentDeployments must be positive.");
        }

        if (config.ResourceQuota.MaxStorageBytes <= 0)
        {
            violations.Add("TenantConfig.ResourceQuota.MaxStorageBytes must be positive.");
        }

        return violations;
    }

    /// <summary>Validates <paramref name="config"/>. Returns every violation found (empty if valid).</summary>
    public static IReadOnlyList<string> ValidateModuleConfig(ModuleConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        var violations = new List<string>();

        if (string.IsNullOrWhiteSpace(config.ModuleId))
        {
            violations.Add("ModuleConfig.ModuleId is missing or empty.");
        }

        if (string.IsNullOrWhiteSpace(config.ModuleName))
        {
            violations.Add("ModuleConfig.ModuleName is missing or empty.");
        }

        foreach (FeatureFlagConfig flag in config.FeatureFlags)
        {
            if (string.IsNullOrWhiteSpace(flag.FlagId))
            {
                violations.Add($"ModuleConfig \"{config.ModuleId}\" has a FeatureFlagConfig with a missing or empty FlagId.");
            }
        }

        return violations;
    }

    /// <summary>
    /// Validates that every ModuleConfig.DependsOnModuleIds reference resolves within
    /// <paramref name="modules"/> and that the dependency graph is acyclic. Returns every violation
    /// found (empty if valid).
    /// </summary>
    public static IReadOnlyList<string> ValidateModuleDependencies(IReadOnlyList<ModuleConfig> modules)
    {
        ArgumentNullException.ThrowIfNull(modules);

        var violations = new List<string>();
        Dictionary<string, ModuleConfig> modulesById = modules.ToDictionary(m => m.ModuleId, StringComparer.Ordinal);

        foreach (ModuleConfig module in modules.OrderBy(m => m.ModuleId, StringComparer.Ordinal))
        {
            foreach (string dependsOnModuleId in module.DependsOnModuleIds)
            {
                if (!modulesById.ContainsKey(dependsOnModuleId))
                {
                    violations.Add($"ModuleConfig \"{module.ModuleId}\" depends on unknown module \"{dependsOnModuleId}\".");
                }
            }
        }

        var visitedState = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (string moduleId in modulesById.Keys.OrderBy(id => id, StringComparer.Ordinal))
        {
            if (visitedState.GetValueOrDefault(moduleId, 0) == 0)
            {
                string? cycleModuleId = DetectDependencyCycle(moduleId, modulesById, visitedState);
                if (cycleModuleId is not null)
                {
                    violations.Add($"Module dependency cycle detected involving \"{cycleModuleId}\".");
                }
            }
        }

        return violations;
    }

    private static string? DetectDependencyCycle(string moduleId, Dictionary<string, ModuleConfig> modulesById, Dictionary<string, int> visitedState)
    {
        visitedState[moduleId] = 1;

        if (modulesById.TryGetValue(moduleId, out ModuleConfig? module))
        {
            foreach (string dependsOnModuleId in module.DependsOnModuleIds)
            {
                int state = visitedState.GetValueOrDefault(dependsOnModuleId, 0);
                if (state == 1)
                {
                    return dependsOnModuleId;
                }

                if (state == 0 && modulesById.ContainsKey(dependsOnModuleId))
                {
                    string? found = DetectDependencyCycle(dependsOnModuleId, modulesById, visitedState);
                    if (found is not null)
                    {
                        return found;
                    }
                }
            }
        }

        visitedState[moduleId] = 2;
        return null;
    }

    /// <summary>
    /// Validates <paramref name="snapshot"/> in full: DeploymentConfig, every TenantConfig, every
    /// RoleConfig (via Deployment.Boundaries.RoleValidator), every ModuleConfig, and module
    /// dependencies. Returns every violation found across the whole snapshot (empty if valid).
    /// </summary>
    public static IReadOnlyList<string> ValidateSnapshot(ConfigSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var violations = new List<string>();

        violations.AddRange(ValidateDeploymentConfig(snapshot.DeploymentConfig));

        foreach (TenantConfig tenant in snapshot.TenantConfigs)
        {
            violations.AddRange(ValidateTenantConfig(tenant));
        }

        foreach (ModuleConfig module in snapshot.ModuleConfigs)
        {
            violations.AddRange(ValidateModuleConfig(module));
        }

        violations.AddRange(ValidateModuleDependencies(snapshot.ModuleConfigs));

        Dictionary<string, RoleConfig> roleCatalog = snapshot.RoleConfigs.ToDictionary(r => r.RoleId, StringComparer.Ordinal);
        violations.AddRange(RoleValidator.ValidateRoleCatalog(roleCatalog));

        return violations;
    }
}
