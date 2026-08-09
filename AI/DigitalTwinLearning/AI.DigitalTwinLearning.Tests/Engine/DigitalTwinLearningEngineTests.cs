// ============================================================================
//  Project:        AI.DigitalTwinLearning.Tests
//  File:           DigitalTwinLearningEngineTests.cs
//  Created:        2026-08-08
//  Description:    Phase E.7 unit tests for DigitalTwinLearningEngine.Learn:
//                  one LearnedPattern per category, anomaly detection, and
//                  rule-contributed model-update recommendations.
// ============================================================================
using FluentAssertions;

using AI.DigitalTwinLearning.Abstractions.Engine;
using AI.DigitalTwinLearning.Abstractions.Learning;
using AI.DigitalTwinLearning.Abstractions.Rules;

using Xunit;

namespace AI.DigitalTwinLearning.Tests.Engine;

public sealed class DigitalTwinLearningEngineTests
{
    private static DigitalTwinLearningInput CreateInput() => new(
        HazardPropagationOccurrences: [1, 1, 1, 1, 1],
        SeverityEscalationOccurrences: [1, 2, 3, 4, 5],
        WorkflowBottleneckOccurrences: [0, 0, 0, 0, 0],
        EmployeeExposureOccurrences: [0, 0, 0, 0, 10],
        MachineFailureOccurrences: [1, 1, 1, 1, 1],
        WindowSize: 4);

    [Fact]
    public void Learn_ReturnsOnePatternPerCategory()
    {
        DigitalTwinLearningOutput output = DigitalTwinLearningEngine.Learn(CreateInput(), []);

        output.Patterns.Should().HaveCount(5);
        output.Patterns.Select(p => p.PatternName).Should().BeEquivalentTo(
            ["HazardPropagation", "SeverityEscalation", "WorkflowBottleneck", "EmployeeExposure", "MachineFailure"]);
    }

    [Fact]
    public void Learn_SeverityEscalation_HasPositiveTrendSlope()
    {
        DigitalTwinLearningOutput output = DigitalTwinLearningEngine.Learn(CreateInput(), []);

        output.Patterns.Single(p => p.PatternName == "SeverityEscalation").TrendSlope.Should().BePositive();
    }

    [Fact]
    public void Learn_EmployeeExposureSpike_IsAnomalous()
    {
        DigitalTwinLearningOutput output = DigitalTwinLearningEngine.Learn(CreateInput(), []);

        output.Patterns.Single(p => p.PatternName == "EmployeeExposure").IsAnomalous.Should().BeTrue();
    }

    [Fact]
    public void Learn_MatchedRule_ContributesRecommendation()
    {
        var rule = new DigitalTwinLearningRule("R1", "Flag anomalies", p => p.IsAnomalous, "Update digital twin exposure model.");

        DigitalTwinLearningOutput output = DigitalTwinLearningEngine.Learn(CreateInput(), [rule]);

        output.ModelUpdateRecommendations.Should().Contain("Update digital twin exposure model.");
    }

    [Fact]
    public void Learn_NoRulesMatch_ReturnsNoRecommendations()
    {
        var rule = new DigitalTwinLearningRule("R1", "Never matches", p => p.TrendSlope > 1000, "Should not appear.");

        DigitalTwinLearningOutput output = DigitalTwinLearningEngine.Learn(CreateInput(), [rule]);

        output.ModelUpdateRecommendations.Should().BeEmpty();
    }
}
