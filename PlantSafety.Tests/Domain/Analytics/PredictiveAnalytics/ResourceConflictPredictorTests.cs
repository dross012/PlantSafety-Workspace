// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Domain.Analytics.PredictiveAnalytics
//  Layer:          Tests (Unit)
//  File:           ResourceConflictPredictorTests.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for ResourceConflictPredictor: weighted composite scoring,
//                  clamping, and guard-clause behavior.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Analytics.PredictiveAnalytics.Predictors;

namespace PlantSafety.Tests.Domain.Analytics.PredictiveAnalytics;

/// <summary>Unit tests for <see cref="ResourceConflictPredictor"/>.</summary>
public sealed class ResourceConflictPredictorTests : UnitTestBase
{
    private readonly ResourceConflictPredictor _sut = new();

    private static ResourceConflictPredictionInput MakeInput(
        int competing = 0, double utilization = 0.0, int sampleCount = 30) =>
        new()
        {
            CompetingCommitmentsInWindow = competing,
            HistoricalUtilizationRatio = utilization,
            HistoricalSampleCount = sampleCount,
        };

    [Fact]
    public void Predict_WithNoCompetitionAndNoUtilization_ShouldReturnZeroLikelihood()
    {
        var result = _sut.Predict(MakeInput(), FixedTimestamp);
        result.Output.ConflictLikelihood.Should().Be(0.0);
    }

    [Fact]
    public void Predict_WithFullUtilizationAndNoCompetition_ShouldReturnUtilizationWeightOnly()
    {
        var result = _sut.Predict(MakeInput(utilization: 1.0), FixedTimestamp);
        result.Output.ConflictLikelihood.Should().Be(0.4);
    }

    [Fact]
    public void Predict_MoreCompetingCommitments_ShouldIncreaseLikelihoodMonotonically()
    {
        var low = _sut.Predict(MakeInput(competing: 1), FixedTimestamp).Output.ConflictLikelihood;
        var high = _sut.Predict(MakeInput(competing: 5), FixedTimestamp).Output.ConflictLikelihood;
        high.Should().BeGreaterThan(low);
    }

    [Fact]
    public void Predict_WithUtilizationAboveOne_ShouldClampContribution()
    {
        var result = _sut.Predict(MakeInput(utilization: 5.0), FixedTimestamp);
        result.Output.ConflictLikelihood.Should().BeLessThanOrEqualTo(1.0);
    }

    [Fact]
    public void Predict_LikelihoodShouldNeverExceedOne()
    {
        var result = _sut.Predict(MakeInput(competing: 100, utilization: 1.0), FixedTimestamp);
        result.Output.ConflictLikelihood.Should().BeLessThanOrEqualTo(1.0);
    }

    [Fact]
    public void Predict_WithNegativeCompetingCommitments_ShouldThrowArgumentOutOfRange()
    {
        var act = () => _sut.Predict(MakeInput(competing: -1), FixedTimestamp);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Predict_WithNullInput_ShouldThrowArgumentNull()
    {
        var act = () => _sut.Predict(null!, FixedTimestamp);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Predict_ShouldPopulateModelNameAndVersion()
    {
        var result = _sut.Predict(MakeInput(), FixedTimestamp);
        result.ModelName.Should().Be("ResourceConflictPredictor");
        result.ModelVersion.Should().Be("2026.1-heuristic");
    }

    [Fact]
    public void Predict_ShouldPopulateRationale()
    {
        var result = _sut.Predict(MakeInput(competing: 2, utilization: 0.5), FixedTimestamp);
        result.Rationale.Should().Contain("2").And.NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Predict_CalledTwiceWithSameInput_ShouldBeDeterministic()
    {
        var input = MakeInput(competing: 3, utilization: 0.6);
        _sut.Predict(input, FixedTimestamp).Output.ConflictLikelihood
            .Should().Be(_sut.Predict(input, FixedTimestamp).Output.ConflictLikelihood);
    }
}
