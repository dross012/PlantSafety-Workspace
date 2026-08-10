// ============================================================================
//  Project:        Deployment.Tests
//  File:           ConfigValidatorTests.cs
//  Created:        2026-08-10
// ============================================================================
using Deployment.ConfigSchema.Contracts;
using Deployment.ConfigSchema.Registry;

using FluentAssertions;

using Xunit;

namespace Deployment.Tests.Unit;

public sealed class ConfigValidatorTests
{
    private static DeploymentContractMetadata Metadata() => new(DeploymentContractMetadata.CurrentVersion, "acme-plant-1", "corr-1", DateTime.UtcNow);

    private static DeploymentConfig ValidBlueGreenConfig() => new(
        Metadata(), "deploy-1", "staging", RolloutStrategyType.BlueGreen, new[] { "module-a" }, Array.Empty<int>(), 30, 3);

    [Fact]
    public void ValidateDeploymentConfig_ValidBlueGreen_ReturnsNoViolations()
    {
        ConfigValidator.ValidateDeploymentConfig(ValidBlueGreenConfig()).Should().BeEmpty();
    }

    [Fact]
    public void ValidateDeploymentConfig_EmptyModuleIds_ReturnsViolation()
    {
        DeploymentConfig config = ValidBlueGreenConfig() with { ModuleIds = Array.Empty<string>() };

        ConfigValidator.ValidateDeploymentConfig(config).Should().NotBeEmpty();
    }

    [Fact]
    public void ValidateDeploymentConfig_CanaryWithNonAscendingStages_ReturnsViolation()
    {
        DeploymentConfig config = ValidBlueGreenConfig() with { Strategy = RolloutStrategyType.Canary, CanaryStagePercentages = new[] { 50, 25, 100 } };

        ConfigValidator.ValidateDeploymentConfig(config).Should().NotBeEmpty();
    }

    [Fact]
    public void ValidateDeploymentConfig_CanaryNotEndingAt100_ReturnsViolation()
    {
        DeploymentConfig config = ValidBlueGreenConfig() with { Strategy = RolloutStrategyType.Canary, CanaryStagePercentages = new[] { 10, 50 } };

        ConfigValidator.ValidateDeploymentConfig(config).Should().NotBeEmpty();
    }

    [Fact]
    public void ValidateDeploymentConfig_ValidCanary_ReturnsNoViolations()
    {
        DeploymentConfig config = ValidBlueGreenConfig() with { Strategy = RolloutStrategyType.Canary, CanaryStagePercentages = new[] { 10, 50, 100 } };

        ConfigValidator.ValidateDeploymentConfig(config).Should().BeEmpty();
    }

    [Fact]
    public void ValidateDeploymentConfig_BlueGreenWithNonEmptyCanaryStages_ReturnsViolation()
    {
        DeploymentConfig config = ValidBlueGreenConfig() with { CanaryStagePercentages = new[] { 50 } };

        ConfigValidator.ValidateDeploymentConfig(config).Should().NotBeEmpty();
    }

    [Fact]
    public void ValidateModuleDependencies_UnknownDependency_ReturnsViolation()
    {
        var modules = new[]
        {
            new ModuleConfig(Metadata(), "module-a", "Module A", ModuleLifecycleState.Enabled, Array.Empty<FeatureFlagConfig>(), new[] { "module-b" }),
        };

        ConfigValidator.ValidateModuleDependencies(modules).Should().NotBeEmpty();
    }

    [Fact]
    public void ValidateModuleDependencies_Cycle_ReturnsViolation()
    {
        var modules = new[]
        {
            new ModuleConfig(Metadata(), "module-a", "Module A", ModuleLifecycleState.Enabled, Array.Empty<FeatureFlagConfig>(), new[] { "module-b" }),
            new ModuleConfig(Metadata(), "module-b", "Module B", ModuleLifecycleState.Enabled, Array.Empty<FeatureFlagConfig>(), new[] { "module-a" }),
        };

        ConfigValidator.ValidateModuleDependencies(modules).Should().Contain(v => v.Contains("cycle"));
    }

    [Fact]
    public void ValidateModuleDependencies_AcyclicChain_ReturnsNoViolations()
    {
        var modules = new[]
        {
            new ModuleConfig(Metadata(), "module-a", "Module A", ModuleLifecycleState.Enabled, Array.Empty<FeatureFlagConfig>(), new[] { "module-b" }),
            new ModuleConfig(Metadata(), "module-b", "Module B", ModuleLifecycleState.Enabled, Array.Empty<FeatureFlagConfig>(), Array.Empty<string>()),
        };

        ConfigValidator.ValidateModuleDependencies(modules).Should().BeEmpty();
    }

    [Fact]
    public void ValidateTenantConfig_NonPositiveQuota_ReturnsViolations()
    {
        var tenant = new TenantConfig(
            Metadata(), "acme-plant-1", "Acme Plant 1", TenantTopology.Shared, TenantStatus.Active,
            new TenantResourceQuota(0, 0, 0), Array.Empty<string>(), Array.Empty<string>());

        ConfigValidator.ValidateTenantConfig(tenant).Should().HaveCount(3);
    }

    [Fact]
    public void ValidateSnapshot_ValidSnapshot_ReturnsNoViolations()
    {
        var tenant = new TenantConfig(
            Metadata(), "acme-plant-1", "Acme Plant 1", TenantTopology.Shared, TenantStatus.Active,
            new TenantResourceQuota(10, 5, 1_000_000), new[] { "module-a" }, Array.Empty<string>());
        var module = new ModuleConfig(Metadata(), "module-a", "Module A", ModuleLifecycleState.Enabled, Array.Empty<FeatureFlagConfig>(), Array.Empty<string>());

        ConfigSnapshot snapshot = ConfigSnapshotFactory.Create(
            "acme-plant-1", 1, ValidBlueGreenConfig(), new[] { tenant }, Array.Empty<RoleConfig>(), new[] { module }, DateTime.UtcNow);

        ConfigValidator.ValidateSnapshot(snapshot).Should().BeEmpty();
    }
}
