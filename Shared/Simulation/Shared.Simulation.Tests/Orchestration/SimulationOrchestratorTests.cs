// ============================================================================
//  Project:        Shared.Simulation.Tests
//  File:           SimulationOrchestratorTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Permissions;
using Shared.Authorization.Abstractions.Roles;
using Shared.Authorization.Abstractions.Tenant;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Simulation.Abstractions.Contracts;
using Shared.Simulation.Abstractions.Simulation;
using Shared.Simulation.Infrastructure.Orchestration;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.Simulation.Tests.Orchestration;

public sealed class SimulationOrchestratorTests
{
    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public List<ILogEvent> Events { get; } = new();
        public void Write(ILogEvent logEvent) => Events.Add(logEvent);
    }

    private static readonly IPermissionResolver Resolver = new EnterprisePermissionResolver();
    private static readonly FixedClock Clock = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    private static readonly CallerContext Caller = new(EnterpriseRole.PlatformAdmin, MfaStatus.NotApplicable, "corr-1");

    private static SimulationEvaluationRequestContract BuildRequest(bool withSteps = true) => new(
        new SimulationMetadata(SimulationMetadata.CurrentSchemaVersion, "acme-plant-1", "corr-1", Clock.UtcNow),
        new SimulationScenario("scenario-1", "Test", withSteps ? new[] { new SimulationStep(0, SimulationStepType.Hazard, "x") } : Array.Empty<SimulationStep>()),
        null, null);

    [Fact]
    public async Task EvaluateAsync_ValidRequest_ReturnsResultWithMatchingTenant()
    {
        var writer = new CapturingLogEventWriter();
        TenantContext tenantContext = TenantContext.Unscoped(Caller, "corr-1");

        SimulationEvaluationResultContract result = await SimulationOrchestrator.EvaluateAsync(
            BuildRequest(), Caller, tenantContext, requestedFacilityId: null, Resolver, tenantFacilityAccessResolver: null, Clock, writer);

        result.Metadata.TenantId.Should().Be("acme-plant-1");
        result.Outcome.Scenario.ScenarioId.Should().Be("scenario-1");
    }

    [Fact]
    public async Task EvaluateAsync_EmitsOrchestrationCompletedAndEvaluationEventPublished()
    {
        var writer = new CapturingLogEventWriter();
        TenantContext tenantContext = TenantContext.Unscoped(Caller, "corr-1");

        await SimulationOrchestrator.EvaluateAsync(
            BuildRequest(), Caller, tenantContext, requestedFacilityId: null, Resolver, tenantFacilityAccessResolver: null, Clock, writer);

        writer.Events.Should().Contain(e => e.EventId == "SIMULATION_ORCHESTRATION_COMPLETED");
        writer.Events.Should().Contain(e => e.EventId == "SIMULATION_EVALUATION_EVENT_PUBLISHED");
        writer.Events.Should().Contain(e => e.EventId == "SIMULATION_REPLAY_PERFORMED");
        writer.Events.Should().Contain(e => e.EventId == "SIMULATION_SCENARIO_EVALUATED");
    }

    [Fact]
    public async Task EvaluateAsync_EmptyScenario_ThrowsSimulationValidationException()
    {
        var writer = new CapturingLogEventWriter();
        TenantContext tenantContext = TenantContext.Unscoped(Caller, "corr-1");

        Func<Task> act = () => SimulationOrchestrator.EvaluateAsync(
            BuildRequest(withSteps: false), Caller, tenantContext, requestedFacilityId: null, Resolver, tenantFacilityAccessResolver: null, Clock, writer);

        (await act.Should().ThrowAsync<SimulationValidationException>())
            .Which.ErrorCode.Should().Be(SimulationErrors.EmptyScenario);
    }

    [Fact]
    public async Task EvaluateAsync_DeniedCaller_ThrowsUnauthorizedAccessException()
    {
        var writer = new CapturingLogEventWriter();
        var deniedCaller = new CallerContext(EnterpriseRole.Viewer, MfaStatus.NotApplicable, "corr-5");
        TenantContext tenantContext = TenantContext.Unscoped(deniedCaller, "corr-5");

        Func<Task> act = () => SimulationOrchestrator.EvaluateAsync(
            BuildRequest(), deniedCaller, tenantContext, requestedFacilityId: null, new AlwaysDenyPermissionResolver(), tenantFacilityAccessResolver: null, Clock, writer);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    private sealed class AlwaysDenyPermissionResolver : IPermissionResolver
    {
        public IReadOnlySet<string> Resolve(EnterpriseRole role, string moduleName) => new HashSet<string>();
        public IReadOnlySet<string> ResolveAcrossModules(EnterpriseRole role, IEnumerable<string> moduleNames) => new HashSet<string>();
    }
}
