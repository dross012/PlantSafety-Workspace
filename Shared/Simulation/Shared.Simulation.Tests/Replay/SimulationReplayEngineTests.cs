// ============================================================================
//  Project:        Shared.Simulation.Tests
//  File:           SimulationReplayEngineTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Simulation.Abstractions.Replay;
using Shared.Simulation.Abstractions.Simulation;

using Xunit;

namespace Shared.Simulation.Tests.Replay;

public sealed class SimulationReplayEngineTests
{
    [Fact]
    public void Replay_ContiguousZeroBasedSteps_IsValid()
    {
        var scenario = new SimulationScenario("scenario-1", "Test", new[]
        {
            new SimulationStep(0, SimulationStepType.Hazard, "first"),
            new SimulationStep(1, SimulationStepType.Routing, "second"),
        });

        SimulationReplayResult result = SimulationReplayEngine.Replay(scenario);

        result.IsValidReplay.Should().BeTrue();
        result.ReplayedStepIndices.Should().Equal(0, 1);
    }

    [Fact]
    public void Replay_OutOfOrderInput_ReplaysInAscendingOrderRegardless()
    {
        var scenario = new SimulationScenario("scenario-1", "Test", new[]
        {
            new SimulationStep(1, SimulationStepType.Routing, "second"),
            new SimulationStep(0, SimulationStepType.Hazard, "first"),
        });

        SimulationReplayResult result = SimulationReplayEngine.Replay(scenario);

        result.ReplayedStepIndices.Should().Equal(0, 1);
        result.IsValidReplay.Should().BeTrue();
    }

    [Fact]
    public void Replay_NonContiguousIndices_IsInvalid()
    {
        var scenario = new SimulationScenario("scenario-1", "Test", new[]
        {
            new SimulationStep(0, SimulationStepType.Hazard, "first"),
            new SimulationStep(2, SimulationStepType.Routing, "skipped one"),
        });

        SimulationReplayResult result = SimulationReplayEngine.Replay(scenario);

        result.IsValidReplay.Should().BeFalse();
        result.AppliedRuleCodes.Should().NotBeEmpty();
    }

    [Fact]
    public void Replay_DuplicateIndices_IsInvalid()
    {
        var scenario = new SimulationScenario("scenario-1", "Test", new[]
        {
            new SimulationStep(0, SimulationStepType.Hazard, "first"),
            new SimulationStep(0, SimulationStepType.Routing, "duplicate"),
        });

        SimulationReplayResult result = SimulationReplayEngine.Replay(scenario);

        result.IsValidReplay.Should().BeFalse();
    }

    [Fact]
    public void Replay_EmptySteps_IsValid()
    {
        var scenario = new SimulationScenario("scenario-1", "Test", Array.Empty<SimulationStep>());

        SimulationReplayEngine.Replay(scenario).IsValidReplay.Should().BeTrue();
    }

    [Fact]
    public void Replay_SameInputTwice_ProducesIdenticalResult()
    {
        var scenario = new SimulationScenario("scenario-1", "Test", new[] { new SimulationStep(0, SimulationStepType.Hazard, "x") });

        SimulationReplayEngine.Replay(scenario).Should().BeEquivalentTo(SimulationReplayEngine.Replay(scenario));
    }

    [Fact]
    public void Replay_NullScenario_Throws()
    {
        Action act = () => SimulationReplayEngine.Replay(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
