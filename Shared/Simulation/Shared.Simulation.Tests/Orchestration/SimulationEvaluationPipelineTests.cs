// ============================================================================
//  Project:        Shared.Simulation.Tests
//  File:           SimulationEvaluationPipelineTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Deterministic.Abstractions.Clock;
using Shared.PredictiveSafety.Abstractions.Common;
using Shared.Simulation.Abstractions.Simulation;
using Shared.Simulation.Infrastructure.Orchestration;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.Simulation.Tests.Orchestration;

public sealed class SimulationEvaluationPipelineTests
{
    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public void Write(ILogEvent logEvent)
        {
        }
    }

    private static readonly FixedClock Clock = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

    [Fact]
    public async Task EvaluateAsync_ValidScenario_ComposesReplayAndScenarioResult()
    {
        var writer = new CapturingLogEventWriter();
        var scenario = new SimulationScenario("scenario-1", "Test", new[] { new SimulationStep(0, SimulationStepType.Hazard, "x") });
        var request = new SimulationEvaluationRequestContract(
            new SimulationMetadata(SimulationMetadata.CurrentSchemaVersion, "acme", "corr-1", Clock.UtcNow), scenario, null, null);

        SimulationEvaluationResultContract result = await SimulationEvaluationPipeline.EvaluateAsync(request, Clock, writer);

        result.Outcome.Replay.IsValidReplay.Should().BeTrue();
        result.Outcome.Scenario.ScenarioId.Should().Be("scenario-1");
    }

    [Fact]
    public async Task EvaluateAsync_InvalidReplay_ForcesCriticalOverallPriority()
    {
        var writer = new CapturingLogEventWriter();
        var scenario = new SimulationScenario("scenario-1", "Test", new[]
        {
            new SimulationStep(0, SimulationStepType.Hazard, "x"),
            new SimulationStep(2, SimulationStepType.Routing, "skipped one"),
        });
        var request = new SimulationEvaluationRequestContract(
            new SimulationMetadata(SimulationMetadata.CurrentSchemaVersion, "acme", "corr-1", Clock.UtcNow), scenario, null, null);

        SimulationEvaluationResultContract result = await SimulationEvaluationPipeline.EvaluateAsync(request, Clock, writer);

        result.Outcome.Replay.IsValidReplay.Should().BeFalse();
        result.Outcome.RequiresOverallEscalation.Should().BeTrue();
        result.Outcome.OverallPriority.Should().Be(PredictivePriority.Critical);
    }

    [Fact]
    public async Task EvaluateAsync_NullRequest_Throws()
    {
        var writer = new CapturingLogEventWriter();

        Func<Task> act = () => SimulationEvaluationPipeline.EvaluateAsync(null!, Clock, writer);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
