// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tenancy.Tenant
//  Layer:          Domain
//  File:           TenantConfiguration.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §2.2 Tenant Configuration)
//  SchemaVersion:  2026.1
//  Description:    Child entity of the Tenant aggregate carrying the tenant's own configuration:
//                  which optional platform modules are enabled, tenant-scoped feature-flag
//                  overrides (see TenantFeatureFlagOverride), and references into the existing
//                  Compliance.RulesEngine (Phase 5) rule set the tenant has opted into. Mutated
//                  only through the owning Tenant aggregate -- never persisted or loaded
//                  independently. Additive-only evolution.
// ============================================================================
using System.Collections.ObjectModel;
using PlantSafety.Domain.Tenancy.Tenant.ValueObjects;

namespace PlantSafety.Domain.Tenancy.Tenant.Entities;

/// <summary>Child entity carrying a Tenant's enabled modules, feature-flag overrides, and compliance rule references.</summary>
public sealed class TenantConfiguration
{
    private readonly List<string> _enabledModules = [];
    private readonly List<TenantFeatureFlagOverride> _featureFlagOverrides = [];
    private readonly List<string> _complianceRuleCodes = [];

    /// <summary>Identifiers of optional platform modules enabled for this tenant (e.g. <c>"Intelligence"</c>, <c>"PredictiveScheduling"</c>).</summary>
    public IReadOnlyList<string> EnabledModules => new ReadOnlyCollection<string>(_enabledModules);

    /// <summary>Tenant-scoped feature-flag overrides.</summary>
    public IReadOnlyList<TenantFeatureFlagOverride> FeatureFlagOverrides => new ReadOnlyCollection<TenantFeatureFlagOverride>(_featureFlagOverrides);

    /// <summary>
    /// Rule codes referencing entries in the existing Compliance.RulesEngine (Phase 5) rule
    /// catalog that this tenant has opted into beyond the platform default set (e.g.
    /// <c>"OSHA.1910.147"</c>, <c>"ISO45001.ClauseA"</c>).
    /// </summary>
    public IReadOnlyList<string> CustomComplianceRuleCodes => new ReadOnlyCollection<string>(_complianceRuleCodes);

    // For rehydration only.
    internal TenantConfiguration() { }

    /// <summary>Creates the default, minimal TenantConfiguration seeded during automated provisioning (§2.1): no optional modules enabled, no feature-flag overrides, no custom compliance rules -- the safest, most restrictive starting point.</summary>
    public static TenantConfiguration CreateDefault() => new();

    /// <summary>Enables an optional platform module for this tenant, if not already enabled.</summary>
    public void EnableModule(string moduleIdentifier)
    {
        if (string.IsNullOrWhiteSpace(moduleIdentifier))
            throw new ArgumentException("TenantConfiguration.EnableModule requires a non-empty module identifier.", nameof(moduleIdentifier));

        var normalized = moduleIdentifier.Trim();
        if (!_enabledModules.Contains(normalized, StringComparer.OrdinalIgnoreCase))
            _enabledModules.Add(normalized);
    }

    /// <summary>Disables a previously enabled optional platform module for this tenant.</summary>
    public void DisableModule(string moduleIdentifier)
        => _enabledModules.RemoveAll(m => string.Equals(m, moduleIdentifier, StringComparison.OrdinalIgnoreCase));

    /// <summary>Sets (adds or replaces) a tenant-scoped feature-flag override.</summary>
    public void SetFeatureFlagOverride(TenantFeatureFlagOverride @override)
    {
        ArgumentNullException.ThrowIfNull(@override);
        _featureFlagOverrides.RemoveAll(f => string.Equals(f.FlagName, @override.FlagName, StringComparison.OrdinalIgnoreCase));
        _featureFlagOverrides.Add(@override);
    }

    /// <summary>Removes a tenant-scoped feature-flag override, reverting to the platform default for that flag.</summary>
    public void ClearFeatureFlagOverride(string flagName)
        => _featureFlagOverrides.RemoveAll(f => string.Equals(f.FlagName, flagName, StringComparison.OrdinalIgnoreCase));

    /// <summary>Adds a custom compliance rule reference for this tenant, if not already present.</summary>
    public void AddComplianceRule(string ruleCode)
    {
        if (string.IsNullOrWhiteSpace(ruleCode))
            throw new ArgumentException("TenantConfiguration.AddComplianceRule requires a non-empty rule code.", nameof(ruleCode));

        var normalized = ruleCode.Trim();
        if (!_complianceRuleCodes.Contains(normalized, StringComparer.OrdinalIgnoreCase))
            _complianceRuleCodes.Add(normalized);
    }

    /// <summary>Removes a custom compliance rule reference for this tenant.</summary>
    public void RemoveComplianceRule(string ruleCode)
        => _complianceRuleCodes.RemoveAll(r => string.Equals(r, ruleCode, StringComparison.OrdinalIgnoreCase));

    /// <summary>Rehydrates a TenantConfiguration from persisted rows. Infrastructure-layer use only.</summary>
    public static TenantConfiguration Rehydrate(
        IEnumerable<string> enabledModules,
        IEnumerable<TenantFeatureFlagOverride> featureFlagOverrides,
        IEnumerable<string> complianceRuleCodes)
    {
        var config = new TenantConfiguration();
        config._enabledModules.AddRange(enabledModules);
        config._featureFlagOverrides.AddRange(featureFlagOverrides);
        config._complianceRuleCodes.AddRange(complianceRuleCodes);
        return config;
    }
}
