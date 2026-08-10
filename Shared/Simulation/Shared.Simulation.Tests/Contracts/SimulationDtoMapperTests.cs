// ============================================================================
//  Project:        Shared.Simulation.Tests
//  File:           SimulationDtoMapperTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.PredictiveSafety.Abstractions.Common;
using Shared.Simulation.Abstractions.Contracts;
using Shared.Simulation.Abstractions.Replay;
using Shared.Simulation.Abstractions.Scenario;
using Shared.Simulation.Abstractions.Simulation;

using Xunit;

namespace Shared.Simulation.Tests.Contracts;

public sealed class SimulationDtoMapperTests
{
    [Fact]
    public void ToDto_ProjectsScenarioReplayAndStepOutcomes()
    {
        var metadata = new SimulationMetadata(SimulationMetadata.CurrentSchemaVersion, "acme", "corr-1", DateTime.UtcNow);
        var replay = new SimulationReplayResult("scenario-1", new[] { 0 }, true, Array.Empty<string>());
        var stepOutcome = new SimulationStepOutcome(0, SimulationStepType.Hazard, true, PredictivePriority.High, new[] { "RULE-1" });
        var scenarioResult = new SimulationScenarioResult("scenario-1", new[] { stepOutcome }, true, PredictivePriority.High, new[] { "RULE-1" });
        var outcome = new SimulationOutcome(replay, scenarioResult, true, PredictivePriority.High);
        var result = new SimulationEvaluationResultContract(metadata, outcome);

        SimulationEvaluationDto dto = SimulationDtoMapper.ToDto(result);

        dto.ScenarioId.Should().Be("scenario-1");
        dto.IsValidReplay.Should().BeTrue();
        dto.StepOutcomes.Should().ContainSingle();
        dto.RequiresOverallEscalation.Should().BeTrue();
        dto.OverallPriority.Should().Be("High");
    }

    [Fact]
    public void ToDto_NullResult_Throws()
    {
        Action act = () => SimulationDtoMapper.ToDto(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
