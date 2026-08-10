// ============================================================================
//  Project:        Deployment.Tests
//  File:           RolloutIntegrationTests.cs
//  Created:        2026-08-10
//  Description:    End-to-end integration coverage across DeploymentEngine,
//                  BlueGreenManager, CanaryManager, ConfigRegistry (publish
//                  + rollback), DriftReconciler, and
//                  DeploymentGovernanceEvaluator (blocking a non-compliant
//                  manifest) -- demonstrating the pieces this phase's
//                  acceptance criteria call out working together, not in
//                  isolation.
// ============================================================================
using Deployment.ConfigSchema.Contracts;
using Deployment.ConfigSchema.Registry;
using Deployment.Governance;
using Deployment.Orchestration;
using Deployment.Orchestration.RolloutStrategies;

using FluentAssertions;

using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Deployment.Tests.Integration;

public sealed class RolloutIntegrationTests
{
    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public List<ILogEvent> Events { get; } = new();
        public void Write(ILogEvent logEvent) => Events.Add(logEvent);
    }

    private static readonly FixedClock Clock = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

    private static DeploymentContractMetadata Metadata() => new(DeploymentContractMetadata.CurrentVersion, "acme-plant-1", "corr-1", Clock.UtcNow);

    private static TenantConfig ActiveTenant() => new(
        Metadata(), "acme-plant-1", "Acme Plant 1", TenantTopology.Shared, TenantStatus.Active,
        new TenantResourceQuota(10, 5, 1_000_000), new[] { "module-a" }, Array.Empty<string>());

    private static ModuleConfig BuildModule() => new(Metadata(), "module-a", "Module A", ModuleLifecycleState.Enabled, Array.Empty<FeatureFlagConfig>(), Array.Empty<string>());

    [Fact]
    public async Task BlueGreenPromotion_ThenDeploymentEngine_ThenDriftReconciliation_Succeeds()
    {
        var writer = new CapturingLogEventWriter();

        // 1. Promote via Blue/Green: idle (Green) passes all gates.
        BlueGreenResult promotion = await BlueGreenManager.PromoteAsync(
            BlueGreenSlot.Blue,
            new[] { new BlueGreenGate("HealthCheck", (_, _) => Task.FromResult(true)) });

        promotion.Promoted.Should().BeTrue();
        promotion.ActiveSlot.Should().Be(BlueGreenSlot.Green);

        // 2. Run the deployment steps for the newly-promoted slot.
        var steps = new[] { new DeploymentStep("switch-traffic", 0, TimeSpan.FromSeconds(5), 0, _ => Task.FromResult(true), null) };
        DeploymentExecutionResult execution = await DeploymentEngine.ExecuteAsync("deploy-1", steps, Clock, writer, "corr-1");
        execution.Succeeded.Should().BeTrue();

        // 3. Publish the resulting config snapshot and reconcile it against itself -- no drift.
        var registry = new ConfigRegistry();
        var deploymentConfig = new DeploymentConfig(Metadata(), "deploy-1", "staging", RolloutStrategyType.BlueGreen, new[] { "module-a" }, Array.Empty<int>(), 30, 3);
        ConfigSnapshot published = registry.Publish(ConfigSnapshotFactory.Create(
            "acme-plant-1", 1, deploymentConfig, new[] { ActiveTenant() }, Array.Empty<RoleConfig>(), new[] { BuildModule() }, Clock.UtcNow));

        DriftReport report = DriftReconciler.ReconcileAgainstRegistry(registry, published, Clock, writer, "corr-1");
        report.DriftDetected.Should().BeFalse();
    }

    [Fact]
    public async Task CanaryRollout_GateFailsMidway_StopsAdvancingAndReportsPartialPromotion()
    {
        CanaryResult result = await CanaryManager.ExecuteAsync(
            new[] { 10, 50, 100 },
            (percentage, _) => Task.FromResult(percentage < 50));

        result.FullyPromoted.Should().BeFalse();
        result.ReachedTrafficPercentage.Should().Be(10);
        result.StageResults.Should().HaveCount(2); // 10 (passed), 50 (failed) -- 100 never attempted
    }

    [Fact]
    public void ConfigRegistry_PublishThenRollback_RestoresPriorSnapshotContentAsNewVersion()
    {
        var registry = new ConfigRegistry();
        var deploymentConfigV1 = new DeploymentConfig(Metadata(), "deploy-1", "staging", RolloutStrategyType.BlueGreen, new[] { "module-a" }, Array.Empty<int>(), 30, 3);
        var deploymentConfigV2 = deploymentConfigV1 with { MaxRetries = 10 };

        ConfigSnapshot v1 = registry.Publish(ConfigSnapshotFactory.Create("acme-plant-1", 1, deploymentConfigV1, new[] { ActiveTenant() }, Array.Empty<RoleConfig>(), new[] { BuildModule() }, Clock.UtcNow));
        registry.Publish(ConfigSnapshotFactory.Create("acme-plant-1", 2, deploymentConfigV2, new[] { ActiveTenant() }, Array.Empty<RoleConfig>(), new[] { BuildModule() }, Clock.UtcNow));

        ConfigSnapshot rolledBack = registry.RollbackTo("acme-plant-1", 1, Clock.UtcNow);

        rolledBack.SnapshotVersion.Should().Be(3); // history extended, never rewritten
        rolledBack.DeploymentConfig.MaxRetries.Should().Be(v1.DeploymentConfig.MaxRetries);
        registry.ListVersions("acme-plant-1").Should().HaveCount(3);
    }

    [Fact]
    public void DeploymentGovernanceEvaluator_BlocksProductionDeploymentWithoutApprovalRole()
    {
        var writer = new CapturingLogEventWriter();
        var productionConfig = new DeploymentConfig(Metadata(), "deploy-1", "production", RolloutStrategyType.BlueGreen, new[] { "module-a" }, Array.Empty<int>(), 30, 3);
        ConfigSnapshot snapshot = ConfigSnapshotFactory.Create(
            "acme-plant-1", 1, productionConfig, new[] { ActiveTenant() }, Array.Empty<RoleConfig>(), new[] { BuildModule() }, Clock.UtcNow);

        DeploymentGovernanceDecision decision = DeploymentGovernanceEvaluator.Evaluate(snapshot, ActiveTenant(), requestedConcurrentDeployments: 1, Clock, writer, "corr-1");

        decision.Allowed.Should().BeFalse();
        writer.Events.Should().Contain(e => e.EventId == "DEPLOYMENT_BLOCKED");
    }

    [Fact]
    public void DeploymentGovernanceEvaluator_AllowsCompliantNonProductionDeployment()
    {
        var writer = new CapturingLogEventWriter();
        var deploymentConfig = new DeploymentConfig(Metadata(), "deploy-1", "staging", RolloutStrategyType.BlueGreen, new[] { "module-a" }, Array.Empty<int>(), 30, 3);
        ConfigSnapshot snapshot = ConfigSnapshotFactory.Create(
            "acme-plant-1", 1, deploymentConfig, new[] { ActiveTenant() }, Array.Empty<RoleConfig>(), new[] { BuildModule() }, Clock.UtcNow);

        DeploymentGovernanceDecision decision = DeploymentGovernanceEvaluator.Evaluate(snapshot, ActiveTenant(), requestedConcurrentDeployments: 1, Clock, writer, "corr-1");

        decision.Allowed.Should().BeTrue();
    }
}
