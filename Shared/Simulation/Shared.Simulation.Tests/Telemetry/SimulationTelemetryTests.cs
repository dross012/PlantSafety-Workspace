// ============================================================================
//  Project:        Shared.Simulation.Tests
//  File:           SimulationTelemetryTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Deterministic.Abstractions.Clock;
using Shared.Simulation.Abstractions.Replay;
using Shared.Simulation.Abstractions.Scenario;
using Shared.Simulation.Abstractions.Simulation;
using Shared.Simulation.Infrastructure.Telemetry;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.Simulation.Tests.Telemetry;

public sealed class SimulationTelemetryTests
{
    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public ILogEvent? LastEvent { get; private set; }
        public void Write(ILogEvent logEvent) => LastEvent = logEvent;
    }

    private static readonly FixedClock Clock = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

    private static SimulationScenario BuildScenario() => new(
        "scenario-1", "Test", new[] { new SimulationStep(0, SimulationStepType.Hazard, "x") });

    [Fact]
    public async Task ReplayWithTelemetryAsync_ReturnsSameResultAsDirectReplay()
    {
        var writer = new CapturingLogEventWriter();
        SimulationScenario scenario = BuildScenario();

        SimulationReplayResult viaWrapper = await SimulationTelemetry.ReplayWithTelemetryAsync(scenario, Clock, writer, "corr-1");
        SimulationReplayResult direct = SimulationReplayEngine.Replay(scenario);

        viaWrapper.Should().BeEquivalentTo(direct);
    }

    [Fact]
    public async Task ReplayWithTelemetryAsync_EmitsReplayPerformedLogEvent()
    {
        var writer = new CapturingLogEventWriter();

        await SimulationTelemetry.ReplayWithTelemetryAsync(BuildScenario(), Clock, writer, "corr-2");

        writer.LastEvent.Should().NotBeNull();
        writer.LastEvent!.EventId.Should().Be("SIMULATION_REPLAY_PERFORMED");
        writer.LastEvent.Category.Should().Be(LogCategory.Simulation);
    }

    [Fact]
    public async Task EvaluateWithTelemetryAsync_EmitsScenarioEvaluatedLogEvent()
    {
        var writer = new CapturingLogEventWriter();
        var context = new SimulationScenarioContext(BuildScenario(), null, null);

        await SimulationTelemetry.EvaluateWithTelemetryAsync(context, Clock, writer, "corr-3");

        writer.LastEvent!.EventId.Should().Be("SIMULATION_SCENARIO_EVALUATED");
    }
}
