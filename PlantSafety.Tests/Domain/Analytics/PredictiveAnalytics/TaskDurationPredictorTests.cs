// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Domain.Analytics.PredictiveAnalytics
//  Layer:          Tests (Unit)
//  File:           TaskDurationPredictorTests.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for TaskDurationPredictor: heuristic duration scaling, confidence
//                  derivation, guard-clause and determinism checks.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Analytics.PredictiveAnalytics.Predictors;

namespace PlantSafety.Tests.Domain.Analytics.PredictiveAnalytics;

/// <summary>Unit tests for <see cref="TaskDurationPredictor"/>.</summary>
public sealed class TaskDurationPredictorTests : UnitTestBase
{
    private readonly TaskDurationPredictor _sut = new();

    private static TaskDurationPredictionInput MakeInput(
        TimeSpan? avg = null, int sampleCount = 30, double complexity = 1.0, double crewExperience = 1.0) =>
        new()
        {
            HistoricalAverageDuration = avg ?? TimeSpan.FromHours(2),
            HistoricalSampleCount = sampleCount,
            ComplexityFactor = complexity,
            CrewExperienceFactor = crewExperience,
        };

    [Fact]
    public void Predict_WithTypicalFactors_ShouldReturnHistoricalAverage()
    {
        var result = _sut.Predict(MakeInput(), FixedTimestamp);
        result.Output.PredictedDuration.Should().Be(TimeSpan.FromHours(2));
    }

    [Fact]
    public void Predict_WithHigherComplexity_ShouldIncreaseDuration()
    {
        var result = _sut.Predict(MakeInput(complexity: 2.0), FixedTimestamp);
        result.Output.PredictedDuration.Should().Be(TimeSpan.FromHours(4));
    }

    [Fact]
    public void Predict_WithHigherCrewExperience_ShouldDecreaseDuration()
    {
        var result = _sut.Predict(MakeInput(crewExperience: 2.0), FixedTimestamp);
        result.Output.PredictedDuration.Should().Be(TimeSpan.FromHours(1));
    }

    [Fact]
    public void Predict_ShouldPopulateModelNameAndVersion()
    {
        var result = _sut.Predict(MakeInput(), FixedTimestamp);
        result.ModelName.Should().Be("TaskDurationPredictor");
        result.ModelVersion.Should().Be("2026.1-heuristic");
    }

    [Fact]
    public void Predict_ShouldStampGeneratedAtUtc()
    {
        var result = _sut.Predict(MakeInput(), FixedTimestamp);
        result.GeneratedAtUtc.Should().Be(FixedTimestamp);
    }

    [Fact]
    public void Predict_WithFullSampleCount_ShouldReturnFullConfidence()
    {
        var result = _sut.Predict(MakeInput(sampleCount: 30), FixedTimestamp);
        result.Confidence.Value.Should().Be(1.0);
    }

    [Fact]
    public void Predict_WithZeroSampleCount_ShouldReturnZeroConfidence()
    {
        var result = _sut.Predict(MakeInput(sampleCount: 0), FixedTimestamp);
        result.Confidence.Value.Should().Be(0.0);
    }

    [Fact]
    public void Predict_WithNullInput_ShouldThrowArgumentNull()
    {
        var act = () => _sut.Predict(null!, FixedTimestamp);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Predict_WithZeroComplexity_ShouldThrowArgumentOutOfRange()
    {
        var act = () => _sut.Predict(MakeInput(complexity: 0), FixedTimestamp);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Predict_WithNegativeComplexity_ShouldThrowArgumentOutOfRange()
    {
        var act = () => _sut.Predict(MakeInput(complexity: -1), FixedTimestamp);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Predict_WithZeroCrewExperience_ShouldThrowArgumentOutOfRange()
    {
        var act = () => _sut.Predict(MakeInput(crewExperience: 0), FixedTimestamp);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Predict_ShouldPopulateRationale()
    {
        var result = _sut.Predict(MakeInput(), FixedTimestamp);
        result.Rationale.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Predict_CalledTwiceWithSameInput_ShouldBeDeterministic()
    {
        var input = MakeInput(complexity: 1.3, crewExperience: 0.9);
        var r1 = _sut.Predict(input, FixedTimestamp);
        var r2 = _sut.Predict(input, FixedTimestamp);
        r1.Output.PredictedDuration.Should().Be(r2.Output.PredictedDuration);
    }
}
