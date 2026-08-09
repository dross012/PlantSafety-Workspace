// ============================================================================
//  Project:        AI.DigitalTwinLearning.Tests
//  File:           DigitalTwinLearningRuleSetTests.cs
//  Created:        2026-08-09
//  Description:    Tests for DigitalTwinLearningRuleSet.Standard, evaluated
//                  through the real DigitalTwinLearningEngine.Learn.
// ============================================================================
using FluentAssertions;

using AI.DigitalTwinLearning.Abstractions.Engine;
using AI.DigitalTwinLearning.Abstractions.Learning;
using AI.DigitalTwinLearning.Abstractions.Rules;

using Xunit;

namespace AI.DigitalTwinLearning.Tests.Rules;

public sealed class DigitalTwinLearningRuleSetTests
{
    [Fact]
    public void Learn_AnomalousSpike_RecommendsRecalibration()
    {
        var input = new DigitalTwinLearningInput(
            HazardPropagationOccurrences: [1, 1, 1, 1, 1],
            SeverityEscalationOccurrences: [1, 1, 1, 1, 1],
            WorkflowBottleneckOccurrences: [0, 0, 0, 0, 0],
            EmployeeExposureOccurrences: [0, 0, 0, 0, 10],
            MachineFailureOccurrences: [1, 1, 1, 1, 1],
            WindowSize: 4);

        DigitalTwinLearningOutput output = DigitalTwinLearningEngine.Learn(input, DigitalTwinLearningRuleSet.Standard);

        output.ModelUpdateRecommendations.Should().Contain(r => r.Contains("recalibrate"));
    }

    [Fact]
    public void Learn_RisingTrend_RecommendsIncreasedFrequency()
    {
        var input = new DigitalTwinLearningInput(
            HazardPropagationOccurrences: [1, 1, 1, 1, 1],
            SeverityEscalationOccurrences: [1, 2, 3, 4, 5],
            WorkflowBottleneckOccurrences: [0, 0, 0, 0, 0],
            EmployeeExposureOccurrences: [0, 0, 0, 0, 0],
            MachineFailureOccurrences: [1, 1, 1, 1, 1],
            WindowSize: 4);

        DigitalTwinLearningOutput output = DigitalTwinLearningEngine.Learn(input, DigitalTwinLearningRuleSet.Standard);

        output.ModelUpdateRecommendations.Should().Contain(r => r.Contains("increase the digital twin's simulated frequency"));
    }

    [Fact]
    public void Learn_FlatLowOccurrences_NoRecommendations()
    {
        var input = new DigitalTwinLearningInput(
            HazardPropagationOccurrences: [0, 0, 0, 0, 0],
            SeverityEscalationOccurrences: [0, 0, 0, 0, 0],
            WorkflowBottleneckOccurrences: [0, 0, 0, 0, 0],
            EmployeeExposureOccurrences: [0, 0, 0, 0, 0],
            MachineFailureOccurrences: [0, 0, 0, 0, 0],
            WindowSize: 4);

        DigitalTwinLearningOutput output = DigitalTwinLearningEngine.Learn(input, DigitalTwinLearningRuleSet.Standard);

        output.ModelUpdateRecommendations.Should().BeEmpty();
    }
}
